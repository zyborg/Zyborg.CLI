using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;

namespace Zyborg.CLI.Binder
{
    /// <summary>
    /// Base class for core CLI model binding behavior.
    /// </summary>
    public abstract class CommandLineBinding
    {
        #region -- Constants --

        /// <summary>
        /// Name of optional method on the model class that would get invoked
        /// after binding is completed, but before executing CLI parsing.
        /// </summary>
        /// <summary>
        /// The method name should match exactly and the method should take a single
        /// parameter of type <see cref="CommandLineApplication"/>.
        /// </summary>
        public const string OnInitMethodName = "Model_OnInit";
        /// <summary>
        /// Name of optional method on the model class that would get invoked
        /// after executing CLI parsing.
        /// </summary>
        /// <summary>
        /// The method name should match exactly and the method should take a single
        /// parameter of type <see cref="CommandLineApplication"/>.
        /// </summary>
        public const string OnExecMethodName = "Model_OnExec";
        /// <summary>
        /// Suffix used to compute the name of optional methods on the model class that would
        /// get invoked after configing bound members.
        /// </summary>
        /// <summary>
        /// The method names are computed by appending this suffix to the name of the member
        /// of the model class that is bing bound for CLI parameter parsing.
        /// The names should match exactly and the methods should take a single
        /// parameter of type equivalent to their CLI parameter type as follows:
        /// <list>
        ///   <item>Option - <see cref="CommandOption"/></item>
        ///   <item>Argument - <see cref="CommandArgument"/></item>
        ///   <item>Command - <see cref="CommandLineApplication"/> (child CLA)</item>
        ///   <item>RemainingArguments - <see cref="CommandLineApplication"/></item>
        /// </list>
        /// </summary>
        public const string OnBindMethodSuffix = "_OnBind";

        #endregion -- Constants --
        
        #region -- Fields --
        
        internal static readonly Type[] OnInitParams = new[] { typeof(CommandLineApplication) };
        internal static readonly Type[] OnExecParams = OnInitParams;
        internal static readonly Type[] OnBindOptionParams = new[] { typeof(CommandOption) };
        internal static readonly Type[] OnBindArgumentParams = new[] { typeof(CommandArgument) };
        internal static readonly Type[] OnBindRemainingArgumentsParams = OnInitParams;
        internal static readonly Type[] OnBindCommandParams = OnInitParams;

        internal static readonly object[] EmptyValues = new object[0];

        internal Dictionary<Type, Action<CommandLineBinding, Attribute, MemberInfo>> _attributeParsers =
            new Dictionary<Type, Action<CommandLineBinding, Attribute, MemberInfo>>
            {
                [typeof(HelpOptionAttribute)] = AddHelpOption,
                [typeof(VersionOptionAttribute)] = AddVersionOption,
                [typeof(ShortVersionGetterAttribute)] = AddVersionOption,
                [typeof(LongVersionGetterAttribute)] = AddVersionOption,

                [typeof(CommandAttribute)] = AddCommand,
                [typeof(OptionAttribute)] = AddOption,
                [typeof(ArgumentAttribute)] = AddArgument,
                [typeof(RemainingArgumentsAttribute)] = AddRemainingArguments,
            };

        internal CommandLineBinding _parentBinding;

        internal CommandLineApplication _cla;
        internal HelpOptionAttribute _helpOption;
        internal VersionOptionAttribute _versionOption;
        internal Func<string> _versionShortGetter;
        internal Func<string> _versionLongGetter;
        internal MemberInfo _remainingArgumentsMember;

        internal bool _executed;

        internal IList<Action> _bindingActions = new List<Action>();
        internal IList<Action> _postExecActions = new List<Action>();

        internal MethodInfo _onExecMethod;

        #endregion -- Fields --
        
        #region -- Properties --

        /// <summary>
        /// Maps to the Out stream for the bound CommandLineApplication.
        /// </summary>
        public TextWriter Out
        {
            get { return _cla?.Out; }
            set { if (_cla != null) _cla.Out = value; }
        }

        /// <summary>
        /// Maps to the Error stream for the bound CommandLineApplication.
        /// </summary>
        public TextWriter Error
        {
            get { return _cla?.Error; }
            set { if (_cla != null) _cla.Error = value; }
        }

        /// <summary>
        /// Resolves the optional full name and version of the bound CommandLineApplication.
        /// </summary>
        public string GetFullNameAndVersion() => _cla?.GetFullNameAndVersion();

        #endregion -- Properties --
        
        #region -- Methods --

        internal int PostExec()
        {
            _executed = true;
            foreach (var action in _postExecActions)
                action();


            var onExecMeth = GetModel().GetType().GetTypeInfo().GetMethod(OnExecMethodName, OnExecParams);
            if (onExecMeth != null)
            {
                var ret = onExecMeth.Invoke(GetModel(), new[] { _cla });
                if (ret != null && typeof(int).GetTypeInfo().IsInstanceOfType(ret))
                    return (int)ret;
            }

            return 0;
        }

        internal static CommandLineBinding BindModel(Type modelType, CommandLineApplication cla = null, Action postExec = null)
        {
            var bindingType = typeof(CommandLineBinding<>).MakeGenericType(modelType);
            var model = Activator.CreateInstance(modelType);
            var binding = (CommandLineBinding)Activator.CreateInstance(bindingType);
            binding.SetModel(model);

            binding.BindTo(cla, postExec);

            return binding;
        }

        internal void BindTo(CommandLineApplication cla, Action postExec = null)
        {
            var binding = this;
            var model = binding.GetModel();
            if (model == null)
                throw new InvalidOperationException("binding has no model instance");

            var modelType = model.GetType();

            binding._cla = cla;
            if (postExec != null)
                binding._postExecActions.Add(postExec);

            SetCLA(binding, modelType.GetTypeInfo().GetCustomAttribute<CommandLineApplicationAttribute>());

            foreach (var a in modelType.GetTypeInfo().GetCustomAttributes())
            {
                var at = a.GetType();
                foreach (var ap in binding._attributeParsers)
                {
                    if (at == ap.Key)
                        ap.Value(binding, a, null);
                }
            }

            foreach (var m in modelType.GetTypeInfo().GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                foreach (var a in m.GetCustomAttributes())
                {
                    var at = a.GetType();
                    foreach (var ap in binding._attributeParsers)
                    {
                        if (at == ap.Key)
                            ap.Value(binding, a, m);
                    }
                }
            }

            foreach (var action in binding._bindingActions)
                action();

            BindRemainingArguments(this);

            binding._cla.OnExecute(() =>
            {
                return binding.PostExec();
            });

            var onInitMeth = modelType.GetTypeInfo().GetMethod(OnInitMethodName, OnInitParams);
            if (onInitMeth != null)
                onInitMeth.Invoke(model, new[] { binding._cla });
        }

        static void SetCLA(CommandLineBinding binding, Attribute att)
        {
            var claAtt = (CommandLineApplicationAttribute)att;
            if (claAtt == null)
                claAtt = new CommandLineApplicationAttribute();

            if (binding._cla == null)
                binding._cla = new CommandLineApplication(claAtt.ThrowOnUnexpectedArg);

            binding._cla.Name = claAtt.Name ?? string.Empty;
            binding._cla.FullName = claAtt.FullName ?? string.Empty;
            binding._cla.Description = claAtt.Description ?? string.Empty;
            binding._cla.AllowArgumentSeparator = claAtt.AllowArgumentSeparator;
        }

        static void AddHelpOption(CommandLineBinding binding, Attribute att, MemberInfo m)
        {
            var a = (HelpOptionAttribute)att;
            binding._helpOption = a;

            binding._bindingActions.Add(() =>
            {
                binding._cla.HelpOption(binding._helpOption.Template);
            });
        }

        static void AddVersionOption(CommandLineBinding binding, Attribute att, MemberInfo m)
        {
            if (att is VersionOptionAttribute)
            {
                binding._versionOption = (VersionOptionAttribute)att;

                binding._bindingActions.Add(() =>
                {
                    if (binding._versionOption == null)
                        return;

                    if (binding._versionShortGetter == null)
                        binding._versionShortGetter = binding._versionLongGetter;

                    if (binding._versionShortGetter != null)
                        binding._cla.VersionOption(binding._versionOption.Template,
                                binding._versionShortGetter, binding._versionLongGetter);
                    else
                        binding._cla.VersionOption(binding._versionOption.Template,
                                binding._versionOption.ShortVersion, binding._versionOption.LongVersion);

                });
            }
            else if (att is ShortVersionGetterAttribute || att is LongVersionGetterAttribute)
            {
                var mi = m as MethodInfo;
                var pi = m as PropertyInfo;
                Func<string> getter = null;

                if (mi != null && mi.ReturnParameter.ParameterType == typeof(string)
                        && mi.GetParameters().Length == 0)
                    getter = () => (string)mi.Invoke(binding.GetModel(), EmptyValues);
                else if (pi != null && pi.PropertyType == typeof(string))
                    getter = () => (string)pi.GetValue(binding.GetModel());

                if (getter != null && att is ShortVersionGetterAttribute)
                    binding._versionShortGetter = getter;
                if (getter != null && att is LongVersionGetterAttribute)
                    binding._versionLongGetter = getter;
            }
        }

        static void AddCommand(CommandLineBinding binding, Attribute att, MemberInfo m)
        {
            var a = (CommandAttribute)att;

            var cmdName = a.Name;
            if (string.IsNullOrEmpty(cmdName))
                cmdName = m.Name.ToLower();

            Type cmdType;
            if (m is MethodInfo)
            {
                var mi = (MethodInfo)m;
                var miParams = mi.GetParameters();

                if (miParams.Length == 1)
                    cmdType = miParams[0].ParameterType;
                else if (miParams.Length == 0)
                    cmdType = null;
                else
                    throw new NotSupportedException("method signature is not supported");
            }
            else if(m is PropertyInfo)
            {
                var pi = (PropertyInfo)m;
                cmdType = pi.PropertyType;
            }
            else
            {
                return;
            }

            // See if there is an optional configuration method for this sub-command
            var onConfigName = $"{m.Name}{OnBindMethodSuffix}";
            var onConfigMeth = binding.GetModel().GetType().GetTypeInfo().GetMethod(
                    onConfigName, OnBindCommandParams);

            // Add the option based on whether there is a config method
            Action<CommandLineApplication> configAction = cla => { };
            if (onConfigMeth != null)
                configAction = cla => onConfigMeth.Invoke(binding.GetModel(), new[] { cla });

            var subCla = binding._cla.Command(cmdName, configAction, a.ThrowOnUnexpectedArg);
            var subBindingType = typeof(CommandLineBinding<>).MakeGenericType(cmdType);

            // When a sub-command is specified, its OnExecute handler is invoked instead of the
            // parent's so we inject a post-exec action to invoke the parent's post-exec actions
            Action parentPostExec = () => binding.PostExec();
            var subBinding = CommandLineBinding.BindModel(cmdType, subCla, parentPostExec);
            subBinding._parentBinding = binding;

            // This is already invoked by Apply which is invoke by Build
            //subModelSetCLA.Invoke(null, new object[] { subModel, cmdType.GetTypeInfo()
            //        .GetCustomAttribute<CommandLineApplicationAttribute>() });

            // We need to make sure the command name from the Command attr is preserved  after
            // processing of the optional CLA attr on the subclass which may have its own name
            subCla.Name = cmdName;

            binding._postExecActions.Add(() => HandleCommand(subCla, subBinding, m,
                    () => subBinding.GetModel()));
        }

        static void HandleCommand(CommandLineApplication cla, CommandLineBinding binding,
                MemberInfo m, Func<object> modelGetter)
        {
            if (!binding._executed)
                return;

            if (m is MethodInfo)
            {
                var mi = (MethodInfo)m;
                var miParams = mi.GetParameters();
                if (miParams.Length == 1)
                    mi.Invoke(binding._parentBinding.GetModel(), new object[] { modelGetter() });
                else if (miParams.Length == 0)
                    mi.Invoke(binding._parentBinding.GetModel(), EmptyValues);

            }
            else if (m is PropertyInfo)
            {
                var pi = (PropertyInfo)m;
                pi.SetValue(binding._parentBinding.GetModel(), modelGetter());
            }
        }

        static void AddOption(CommandLineBinding binding, Attribute att, MemberInfo m)
        {
            var a = (OptionAttribute)att;

            // Resolve the option value type
            Type valueType = null;
            if (m is PropertyInfo)
            {
                var pi = (PropertyInfo)m;
                valueType = pi.PropertyType;
            }
            else if (m is MethodInfo)
            {
                var mi = (MethodInfo)m;
                var miParams = mi.GetParameters();

                if (miParams.Length == 1 || (miParams.Length == 2
                        && typeof(CommandLineBinding).GetTypeInfo().IsAssignableFrom(miParams[1].ParameterType)))
                    valueType = miParams[0].ParameterType;
                else if (miParams.Length > 0)
                    throw new NotSupportedException("method signature is not supported");
            }

            // Figure out the option type based on value type if it wasn't explicitly specified
            CommandOptionType optionType = CommandOptionType.NoValue;
            if (a.OptionType == null && valueType != null)
            {
                // Try to resolve the option type based on the property type
                if (valueType.GetTypeInfo().IsAssignableFrom(typeof(string)))
                {
                    optionType = CommandOptionType.SingleValue;
                }
                else if (valueType.GetTypeInfo().IsAssignableFrom(typeof(string[])))
                {
                    optionType = CommandOptionType.MultipleValue;
                }
                else if (valueType == typeof(bool?) || valueType == typeof(bool))
                {
                    optionType = CommandOptionType.NoValue;
                }
                else
                    throw new NotSupportedException("option value type is not supported");
            }

            // Resolve the option template if it wasn't explicitly specified
            var template = a.Template;
            if (string.IsNullOrEmpty(template))
                template = $"--{m.Name.ToLower()}";

            // See if there is an optional configuration method for this option
            var onConfigName = $"{m.Name}{OnBindMethodSuffix}";
            var onConfigMeth = binding.GetModel().GetType().GetTypeInfo().GetMethod(
                    onConfigName, OnBindOptionParams);

            // Add the option based on whether there is a config method
            CommandOption co;
            if (onConfigMeth != null)
            {
                co = binding._cla.Option(template, a.Description,
                        optionType,
                        cmdOpt => onConfigMeth.Invoke(binding.GetModel(), new[] { cmdOpt }),
                        a.Inherited);
            }
            else
            {
                co = binding._cla.Option(template, a.Description,
                        optionType,
                        a.Inherited);
            }

            // Add a post-exec handler for this option
            binding._postExecActions.Add(() => HandleOption(co, binding, m));
        }

        static void HandleOption(CommandOption co, CommandLineBinding binding, MemberInfo m)
        {
            if (!co.HasValue())
                return;

            object coValue;
            if (co.OptionType == CommandOptionType.NoValue)
            {
                //coValue = bool.Parse(co.Value());
                coValue = true;
            }
            else if (co.OptionType == CommandOptionType.SingleValue)
            {
                coValue = co.Value();
            }
            else
            {
                coValue = co.Values.ToArray();
            }

            if (m is MethodInfo)
            {
                var mi = (MethodInfo)m;
                var miParams = mi.GetParameters();
                if (miParams.Length == 2)
                    mi.Invoke(binding.GetModel(), new object[] { coValue, binding });
                else if (miParams.Length == 1)
                    mi.Invoke(binding.GetModel(), new object[] { coValue });
                else
                    mi.Invoke(binding.GetModel(), EmptyValues);
            }
            else if (m is PropertyInfo)
            {
                var pi = (PropertyInfo)m;
                pi.SetValue(binding.GetModel(), coValue);
            }
        }

        static void AddArgument(CommandLineBinding binding, Attribute att, MemberInfo m)
        {
            var a = (ArgumentAttribute)att;

            var onConfigName = $"{m.Name}{OnBindMethodSuffix}";
            var onConfigMeth = binding.GetModel().GetType().GetTypeInfo().GetMethod(
                    onConfigName, OnBindArgumentParams);

            CommandArgument ca;
            if (onConfigMeth != null)
            {
                ca = binding._cla.Argument(a.Name, a.Description,
                        cmdArg => onConfigMeth.Invoke(binding.GetModel(), new[] { cmdArg }),
                        a.MultipleValues);
            }
            else
            {
                ca = binding._cla.Argument(a.Name, a.Description, a.MultipleValues);
            }

            binding._postExecActions.Add(() => { HandleArgument(ca, binding, m); });
        }

        static void HandleArgument(CommandArgument ca, CommandLineBinding binding, MemberInfo m)
        {
            if (ca.Values?.Count == 0)
                return;

            object caValue;
            if (ca.MultipleValues)
                caValue = ca.Values.ToArray();
            else
                caValue = ca.Value;

            if (m is MethodInfo)
            {
                var mi = (MethodInfo)m;
                var miParams = mi.GetParameters();
                if (miParams.Length == 2)
                    mi.Invoke(binding.GetModel(), new object[] { caValue, binding });
                else
                    mi.Invoke(binding.GetModel(), new object[] { caValue });
            }
            else if (m is PropertyInfo)
            {
                var pi = (PropertyInfo)m;
                pi.SetValue(binding.GetModel(), caValue);
            }
        }

        static void AddRemainingArguments(CommandLineBinding binding, Attribute att, MemberInfo m)
        {
            var a = (RemainingArgumentsAttribute)att;
            binding._remainingArgumentsMember = m;
        }

        static void BindRemainingArguments(CommandLineBinding binding)
        {
            var m = binding._remainingArgumentsMember;
            if (m == null)
                return;

            var a = (RemainingArgumentsAttribute)m.GetCustomAttribute(typeof(RemainingArgumentsAttribute));
            var onConfigName = $"{m.Name}{OnBindMethodSuffix}";
            var onConfigMeth = binding.GetModel().GetType().GetTypeInfo().GetMethod(
                    onConfigName, OnBindRemainingArgumentsParams);

            if (onConfigMeth != null)
            {
                onConfigMeth.Invoke(binding.GetModel(), new[] { binding._cla });
            }

            binding._postExecActions.Add(() => { HandleRemainingArguments(a, binding, m); });
        }

        static void HandleRemainingArguments(RemainingArgumentsAttribute att,
                CommandLineBinding binding, MemberInfo m)
        {
            if (att.SkipIfNone && binding._cla.RemainingArguments?.Count == 0)
                // No remaining args
                return;

            var args = binding._cla.RemainingArguments?.ToArray();
            if (args == null)
                args = new string[0];

            if (m is MethodInfo)
            {
                var mi = (MethodInfo)m;
                mi.Invoke(binding.GetModel(), args);
            }
            else if (m is PropertyInfo)
            {
                var pi = (PropertyInfo)m;
                pi.SetValue(binding.GetModel(), args);
            }
        }

        internal abstract object GetModel();

        internal abstract void SetModel(object model);

        #endregion -- Methods --
    }

    /// <summary>
    /// Type-safe implementation of a model-bound Command Line Interface parameter parser.
    /// </summary>
    public class CommandLineBinding<T> : CommandLineBinding
    {
        public T Model
        { get; internal set; }

        internal override object GetModel() => Model;

        internal override void SetModel(object model) => Model = (T)model;

        public static CommandLineBinding<T> Build()
        {
            return (CommandLineBinding<T>)BindModel(typeof(T));
        }

        public int Execute(params string[] args)
        {
            return _cla.Execute(args);
        }
    }
}