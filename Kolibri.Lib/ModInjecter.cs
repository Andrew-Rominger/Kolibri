﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Inject;

namespace Kolibri.Lib
{
    public class ModInjecter
    {
        private readonly ModManager _modManager = new ModManager();
        private readonly DefaultAssemblyResolver _resolver;
        private readonly DirectoryInfo _gameDirectory;
        private readonly DirectoryInfo _gameAssemblyDirectory;
        private readonly FileInfo _gameAssemblyFile;
        private readonly FileInfo _backupGameAssemblyFile;
        private readonly List<FileInfo> _modAssemblies = new List<FileInfo>();

        public ModInjecter(string gameDirectory)
        {
            _gameDirectory = new DirectoryInfo(gameDirectory);

            var modDir = new DirectoryInfo(Path.Combine(_gameDirectory.FullName, "Mods"));

            if (!modDir.Exists)
                throw new Exception($"Unable to find a Mods folder in {modDir.FullName}");

            var modDirectories = modDir
                .GetDirectories();

            if(!modDirectories.Any())
                throw new Exception($"No mod folders found in {modDir.FullName}");

            foreach (var modDirectory in modDirectories)
            {
                var modAssembly = modDirectory.GetFiles($"{modDirectory.Name}.dll").SingleOrDefault();
                if(modAssembly == null || !modAssembly.Exists)
                    throw new FileNotFoundException($"Could not find the mod file for {modDirectory.FullName}");
                _modAssemblies.Add(modAssembly);
            }
            
            _gameAssemblyDirectory = _gameDirectory
                .GetDirectories("*Data")
                .FirstOrDefault()?
                .GetDirectories("Managed")
                .FirstOrDefault();

            if(_gameAssemblyDirectory == null)
                throw new NullReferenceException("Game's data directory was not found");

            _resolver = new DefaultAssemblyResolver();
            _resolver.AddSearchDirectory(_gameAssemblyDirectory.FullName);
            _resolver.AddSearchDirectory(_gameDirectory.FullName);

            _gameAssemblyFile = _gameAssemblyDirectory
                .GetFiles("Assembly-CSharp.dll")
                .Single();

            _backupGameAssemblyFile = new FileInfo(Path.Combine(_gameAssemblyDirectory.FullName, $"Assembly-CSharp-Backup.dll"));

            if (!_backupGameAssemblyFile.Exists)
            {
                var gameAssembly = AssemblyDefinition.ReadAssembly(_gameAssemblyFile.FullName,
                    new ReaderParameters() {AssemblyResolver = _resolver});
                gameAssembly.Write(_backupGameAssemblyFile.FullName);
                
            }
            else
            {
                File.Copy(_backupGameAssemblyFile.FullName, _gameAssemblyFile.FullName, true);
            }

            LoadUnityAssemblies();
            foreach (var modAssembly in _modAssemblies)
            {
                _modManager.AddMod(new Mod(modAssembly,_gameDirectory));
            }

            Console.WriteLine($"***************************************");
            Console.WriteLine($"Done loading mods. Ready to inject");
            Console.WriteLine($"***************************************");
        }

        private void LoadUnityAssemblies()
        {
            Assembly.LoadFrom(_backupGameAssemblyFile.FullName);
            Assembly.LoadFrom(Path.Combine(_gameAssemblyFile.DirectoryName, "UnityEngine.dll"));
            
        }

        public void Inject()
        {
            Console.WriteLine("Beginning inject\n");
            var gameAssembly = AssemblyDefinition.ReadAssembly(_backupGameAssemblyFile.FullName,new ReaderParameters() {AssemblyResolver = _resolver});

            foreach (var mod in _modManager)
            {
                Console.WriteLine($"\t_______________________________________________\n");
                Console.Write($"\tInjecting {Path.GetFileNameWithoutExtension(mod.ModAssemblyFile.Name)}...");
                var modAssembly = AssemblyDefinition.ReadAssembly(mod.ModAssemblyFile.FullName, new ReaderParameters(){AssemblyResolver = _resolver});
                   
                foreach (var methodInjection in mod.ModMethodInjections)
                {
                    var toInject = modAssembly
                        .MainModule
                        .GetType(methodInjection.SourceMethod.InjectionType)
                        .Methods
                        .Single(m => m.Name == methodInjection.SourceMethod.InjectionMethod);
                    if (methodInjection.InjectionAssembly != null)
                    {
                        var injectPath = new FileInfo(Path.Combine(_gameAssemblyFile.Directory.FullName,
                            methodInjection.InjectionAssembly));
                        var tempFil = new FileInfo(Path.Combine(injectPath.DirectoryName, injectPath.Name+ "_temp.dll"));
                        var tempFil2 = new FileInfo(Path.Combine(injectPath.DirectoryName, injectPath.Name+ "_temp2.dll"));
                        File.Copy(injectPath.FullName, tempFil.FullName, true);

                        var injectTargetAssembly = AssemblyDefinition.ReadAssembly(tempFil.FullName,new ReaderParameters { AssemblyResolver = _resolver });

                        
                        var injectionLocation = injectTargetAssembly
                            .MainModule
                            .GetType(methodInjection.TargetMethod.InjectionType)
                            .GetMethod(methodInjection.TargetMethod.InjectionMethod);

                        var injector = new InjectionDefinition(injectionLocation, toInject, methodInjection.InjectFlags);

                        injector
                            .Inject(methodInjection.InjectionLocation == MethodInjectionInfo.MethodInjectionLocation.Top
                                ? injectionLocation.Body.Instructions.First()
                                : injectionLocation.Body.Instructions.Last());
                        injectTargetAssembly.Write(tempFil2.FullName);

                    }
                    else
                    {
                        var injectionLocation = gameAssembly
                            .MainModule
                            .GetType(methodInjection.TargetMethod.InjectionType)
                            .GetMethod(methodInjection.TargetMethod.InjectionMethod);

                        var injector = new InjectionDefinition(injectionLocation, toInject, methodInjection.InjectFlags);

                        injector
                            .Inject(methodInjection.InjectionLocation == MethodInjectionInfo.MethodInjectionLocation.Top
                                ? injectionLocation.Body.Instructions.First()
                                : injectionLocation.Body.Instructions.Last());
                    }
                }
                Console.WriteLine($"Done.\n\tCopying Dependencies");
                File.Copy(mod.ModAssemblyFile.FullName, Path.Combine(_gameDirectory.FullName, mod.ModAssemblyFile.Name), true);
                File.Copy(mod.ModAssemblyFile.FullName, Path.Combine(_gameAssemblyDirectory.FullName, mod.ModAssemblyFile.Name), true);
                if (mod.ModDependencyDirectory != null)
                {
                    foreach (var file in mod.ModDependencyDirectory.GetFiles())
                    {
                        try
                        {
                            File.Copy(file.FullName, Path.Combine(_gameDirectory.FullName, file.Name), true);
                            File.Copy(file.FullName, Path.Combine(_gameAssemblyDirectory.FullName, file.Name), true);
                        }
                        catch (Exception e)
                        {
                           Console.WriteLine($"\t\tUnable to copy {file.Name}, probally already there. Skipping...");
                        }
                        
                    }
                }
                Console.WriteLine($"\t_______________________________________________\n");
            }
            Console.WriteLine($"***************************************");
            Console.WriteLine($"Done injecting. Ready to write modified assembly");
            Console.WriteLine($"***************************************");
            gameAssembly.Write(_gameAssemblyFile.FullName);
        }
    }
}
