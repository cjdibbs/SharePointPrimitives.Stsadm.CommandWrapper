﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using log4net;
using log4net.Config;
using Microsoft.SharePoint.StsAdmin;

namespace SharePointPrimitives.Stsadm {
    /// <summary>
    /// Base Class for Stsadm Commands. Works on SharePoint 2010 with a binding redirect
    /// </summary>
    public abstract class BaseCommand : ISPStsadmCommand {

        protected ILog Log { get; private set; }
        protected TextWriter Out { get; private set; }
        /// <summary>
        /// Shown above the command argument help, intended to
        /// give a discrption of how the command works and what it does
        /// </summary>
        protected virtual string HelpDescription { get { return null; } }

        /// <summary>
        /// Shown below the command argument help. intended to
        /// show examples of how to use the commmand
        /// </summary>
        protected virtual string HelpExamples { get { return null; } }

        /// <summary>
        /// Takes the HelpDescription, Help for each command, and the HelpExamples and
        /// build an over all help message
        /// </summary>
        /// <param name="command">Name of the command</param>
        /// <returns>Help message consturcted from the arguments</returns>
        public string GetHelpMessage(string command) {
            string ret = "";
            if (!String.IsNullOrEmpty(HelpDescription))
                ret += "\n" + HelpDescription;
            ret += "\n" + string.Join("\n", BaseArguments.Select(arg => String.Format("-{0}\n\t{1}", arg.Name, arg.Help)).ToArray());
            if(!String.IsNullOrEmpty(HelpExamples))
                ret += "\n" + HelpExamples;
            return ret;
        }

        /// <summary>
        /// adds in the default commands, and then appends on any commands from the
        /// base class
        /// </summary>
        private IEnumerable<CommandArgument> BaseArguments {
            get {
                yield return new CommandArgument() {
                    Name = "log4net",
                    CommandRequired = false,
                    ArgumentRequired = true,
                    Help = "sets the uri to load a log4net conf file from",
                    OnCommand = uri => XmlConfigurator.Configure(new Uri(uri))
                };

                foreach (var arg in CommandArguments)
                    yield return arg;
            }
        }

        /// <summary>
        /// Any custom commands needed by the base class
        /// </summary>
        abstract protected IEnumerable<CommandArgument> CommandArguments { get; }

        /// <summary>
        /// The custom command. this will be called after all of the commands
        /// arguments have been dispatched
        /// </summary>
        /// <param name="command">name of the command</param>
        /// <returns>an int value that is bubbled up to Stsadm</returns>
        abstract protected int Run(string command);

        /// <summary>
        /// Base run method
        /// 
        /// sets up log4net, either by the command line argument or useing the
        /// BasicConfigurator class.
        /// 
        /// Creates a TextWriter for the output
        /// 
        /// Dispatches the command line arguments, while gathering up all of the errors
        /// not just the first one like the built in command do
        /// </summary>
        /// <param name="command">name of the command</param>
        /// <param name="args">args to the command</param>
        /// <param name="output">output string for the command</param>
        /// <returns></returns>
        public int Run(string command, StringDictionary args, out string output) {
            Out = new StringWriter();
            int ret = 0;
            bool dispatchError = DispatchCommandArguments(args);

            if (!LogManager.GetRepository().Configured)
                BasicConfigurator.Configure();

            Log = LogManager.GetLogger(GetType());
            try {
                if (!dispatchError)
                    ret = Run(command);
            } catch (Exception e) {
                Log.Fatal(e.Message, e);
            } finally {
                output = Out.ToString();
            }
            return ret;
        }

        /// <summary>
        /// Dispatches the command arguments gathering all of the errors rather than just failing on the first one
        /// </summary>
        /// <param name="args">argumetns passes in to the Stsadm command</param>
        /// <returns>if any error happened</returns>
        private bool DispatchCommandArguments(StringDictionary args) {
            var requried = BaseArguments.Where(arg => arg.CommandRequired).Select(arg => arg.Name);
            var lookup = BaseArguments.ToDictionary(arg => arg.Name);
            bool dispatchError = false;

            foreach (var name in requried) {
                if (!args.ContainsKey(name)) {
                    if (!dispatchError) {
                        dispatchError = true;
                        Out.WriteLine("Missing Commands:");
                    }
                    Out.WriteLine("-{0}:\n\t{1}", name, lookup[name].Help);
                }
            }

            foreach (string name in args.Keys) {
                if (name == "o") 
                    continue;
            
                string value = args[name];
                if (!lookup.ContainsKey(name)) {
                    dispatchError = true;
                    Out.WriteLine("Unknown Argument -{0} {1}", name, value);
                } else {
                    CommandArgument arg = lookup[name];
                    if (String.IsNullOrEmpty(value) && arg.ArgumentRequired) {
                        dispatchError = true;
                        Out.WriteLine("Missing Missing Command Argument for -{0}", name);
                    } else
                        arg.OnCommand(value);
                }
            }
            return dispatchError;
        }
    }
}
