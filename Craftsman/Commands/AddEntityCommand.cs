﻿namespace Craftsman.Commands
{
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Repositories;
    using Craftsman.Builders.Seeders;
    using Craftsman.Builders.Tests.Fakes;
    using Craftsman.Builders.Tests.IntegrationTests;
    using Craftsman.Builders.Tests.RepositoryTests;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Linq;
    using static Helpers.ConsoleWriter;

    public static class AddEntityCommand
    {
        public static void Help()
        {
            WriteHelpHeader(@$"Description:");
            WriteHelpText(@$"   While in your project directory, this command can add one or more new entities to your Accelerate Core project based on a formatted yaml or json file. The file input uses a simplified format from the `new:api` command, only requiring a list of one or more entities.{Environment.NewLine}");

            WriteHelpHeader(@$"Usage:");
            WriteHelpText(@$"   craftsman add:entity [options] <filepath>{Environment.NewLine}");

            WriteHelpText(@$"   For example:");
            WriteHelpText(@$"       craftsman add:entity C:\fullpath\newentity.yaml");
            WriteHelpText(@$"       craftsman add:entity C:\fullpath\newentity.yml");
            WriteHelpText(@$"       craftsman add:entity C:\fullpath\newentity.json{Environment.NewLine}");

            WriteHelpHeader(@$"Options:");
            WriteHelpText(@$"   -h, --help          Display this help message. No filepath is needed to display the help message.");
        }

        public static void Run(string filePath, string solutionDirectory)
        {
            try
            {
                GlobalSingleton instance = GlobalSingleton.GetInstance();

                FileParsingHelper.RunInitialTemplateParsingGuards(filePath);
                var template = FileParsingHelper.GetApiTemplateFromFile(filePath);

                //var solutionDirectory = Directory.GetCurrentDirectory();
                //var solutionDirectory = @"C:\Users\Paul\Documents\testoutput\MyApi.Mine";
                template = SolutionGuard(solutionDirectory, template);
                template = GetDbContext(solutionDirectory, template);

                WriteHelpText($"Your template file was parsed successfully.");

                FileParsingHelper.RunKeyGuard(template);

                // add all files based on the given template config
                RunEntityBuilders(solutionDirectory, template);

                WriteFileCreatedUpdatedResponse();
                WriteHelpHeader($"{Environment.NewLine}Your entities have been successfully added. Keep up the good work!");
            }
            catch (Exception e)
            {
                if (e is FileAlreadyExistsException
                    || e is DirectoryAlreadyExistsException
                    || e is InvalidSolutionNameException
                    || e is FileNotFoundException
                    || e is InvalidDbProviderException
                    || e is InvalidFileTypeException
                    || e is SolutionNotFoundException)
                {
                    WriteError($"{e.Message}");
                }
                else
                    WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
            }
        }

        private static void RunEntityBuilders(string solutionDirectory, ApiTemplate template)
        {
            //entities
            foreach (var entity in template.Entities)
            {
                EntityBuilder.CreateEntity(solutionDirectory, entity);
                DtoBuilder.CreateDtos(solutionDirectory, entity);

                RepositoryBuilder.AddRepository(solutionDirectory, entity, template.DbContext);
                ValidatorBuilder.CreateValidators(solutionDirectory, entity);
                ProfileBuilder.CreateProfile(solutionDirectory, entity);

                ControllerBuilder.CreateController(solutionDirectory, entity);

                FakesBuilder.CreateFakes(solutionDirectory, template, entity);
                ReadTestBuilder.CreateEntityReadTests(solutionDirectory, template, entity);
                GetTestBuilder.CreateEntityGetTests(solutionDirectory, template, entity);
                PostTestBuilder.CreateEntityWriteTests(solutionDirectory, template, entity);
                UpdateTestBuilder.CreateEntityUpdateTests(solutionDirectory, template, entity);
                DeleteTestBuilder.DeleteEntityWriteTests(solutionDirectory, template, entity);
            }

            //seeders & dbsets
            SeederModifier.AddSeeders(solutionDirectory, template);
            DbContextModifier.AddDbSet(solutionDirectory, template);
        }

        private static string GetSlnFile(string filePath)
        {
            // make sure i'm in the sln directory -- should i add an accelerate.config.yaml file to the root?
            return Directory.GetFiles(filePath, "*.sln").FirstOrDefault();
        }

        private static ApiTemplate SolutionGuard(string solutionDirectory, ApiTemplate template)
        {
            var slnName = GetSlnFile(solutionDirectory);
            template.SolutionName = Path.GetFileNameWithoutExtension(slnName) ?? throw new SolutionNotFoundException();

            return template;
        }

        private static ApiTemplate GetDbContext(string solutionDirectory, ApiTemplate template)
        {
            var classPath = ClassPathHelper.DbContextClassPath(solutionDirectory, $"");
            var contextClass = Directory.GetFiles(classPath.FullClassPath, "*.cs").FirstOrDefault();

            template.DbContext.ContextName = Path.GetFileNameWithoutExtension(contextClass);
            return template;
        }
    }
}
