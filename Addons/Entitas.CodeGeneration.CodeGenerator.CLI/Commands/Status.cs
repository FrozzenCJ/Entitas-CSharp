﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entitas.Utils;
using Fabl;

namespace Entitas.CodeGeneration.CodeGenerator.CLI {

    public static class Status {

        public static void Run(Properties properties) {
            if(File.Exists(Preferences.PATH)) {
                var config = new CodeGeneratorConfig();
                config.Configure(properties);

                fabl.Debug(config.ToString());

                Type[] types = null;
                Dictionary<string, string> configurables = null;

                try {
                    types = CodeGeneratorUtil.LoadTypesFromPlugins(properties);
                    configurables = CodeGeneratorUtil.GetConfigurables(
                        CodeGeneratorUtil.GetUsed<ICodeGeneratorDataProvider>(types, config.dataProviders),
                        CodeGeneratorUtil.GetUsed<ICodeGenerator>(types, config.codeGenerators),
                        CodeGeneratorUtil.GetUsed<ICodeGenFilePostProcessor>(types, config.postProcessors)
                    );
                } catch(Exception ex) {
                    printKeyStatus(null, properties);
                    throw ex;
                }

                printKeyStatus(configurables, properties);
                printConfigurableKeyStatus(configurables, properties);
                printPluginStatus(types, config);
            } else {
                PrintNoConfig.Run();
            }
        }

        static void printKeyStatus(Dictionary<string, string> configurables, Properties properties) {
            var requiredKeys = new CodeGeneratorConfig().defaultProperties.Keys.ToArray();
            var requiredKeysWithConfigurable = new CodeGeneratorConfig().defaultProperties.Keys.ToArray();

            if(configurables != null) {
                requiredKeysWithConfigurable = requiredKeysWithConfigurable.Concat(configurables.Keys).ToArray();
            }

            foreach(var key in Helper.GetUnusedKeys(requiredKeysWithConfigurable, properties)) {
                fabl.Info("Unused key: " + key);
            }

            foreach(var key in Helper.GetMissingKeys(requiredKeys, properties)) {
                fabl.Warn("Missing key: " + key);
            }
        }

        static void printConfigurableKeyStatus(Dictionary<string, string> configurables, Properties properties) {
            foreach(var kv in CodeGeneratorUtil.GetMissingConfigurables(configurables, properties)) {
                fabl.Warn("Missing key: " + kv.Key);
            }
        }

        static void printPluginStatus(Type[] types, CodeGeneratorConfig config) {
            var unavailableDataProviders = CodeGeneratorUtil.GetUnavailable<ICodeGeneratorDataProvider>(types, config.dataProviders);
            var unavailableCodeGenerators = CodeGeneratorUtil.GetUnavailable<ICodeGenerator>(types, config.codeGenerators);
            var unavailablePostProcessors = CodeGeneratorUtil.GetUnavailable<ICodeGenFilePostProcessor>(types, config.postProcessors);

            var availableDataProviders = CodeGeneratorUtil.GetAvailable<ICodeGeneratorDataProvider>(types, config.dataProviders);
            var availableCodeGenerators = CodeGeneratorUtil.GetAvailable<ICodeGenerator>(types, config.codeGenerators);
            var availablePostProcessors = CodeGeneratorUtil.GetAvailable<ICodeGenFilePostProcessor>(types, config.postProcessors);

            printUnavailable(unavailableDataProviders);
            printUnavailable(unavailableCodeGenerators);
            printUnavailable(unavailablePostProcessors);

            printAvailable(availableDataProviders);
            printAvailable(availableCodeGenerators);
            printAvailable(availablePostProcessors);
        }

        static void printUnavailable(string[] names) {
            foreach(var name in names) {
                fabl.Warn("Unavailable: " + name);
            }
        }

        static void printAvailable(string[] names) {
            foreach(var name in names) {
                fabl.Info("Available: " + name);
            }
        }
    }
}