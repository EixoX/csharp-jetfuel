﻿using EixoX.RocketLauncher.DatabaseGathering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EixoX.RocketLauncher.Command
{
    /// <summary>
    /// Command logic for creating classes from a database source
    /// </summary>
    public class ClassesFromDatabaseCommand : SettingsBasedCommand, ICommand
    {
        public ClassGenerator ClassGenerator { get; set; }
        public ProgrammingLanguage Language { get; set; }
        public IRocketLauncherView View { get; set; }
        private string _Directory { get; set; }

        /// <summary>
        /// Gets the command enum value it uses
        /// </summary>
        public Commands Command
        {
            get { return Commands.ClassesFromDatabase; }
        }

        public ClassesFromDatabaseCommand(ProgrammingLanguage language, string directory, IRocketLauncherView view)
        {
            this.Language = language;
            this.View = view;
            this._Directory = directory;

            switch (language)
            {
                case ProgrammingLanguage.Csharp:
                    this.ClassGenerator = new CSharpClassGenerator(directory);
                    break;
                case ProgrammingLanguage.Java:
                    break;
                case ProgrammingLanguage.VBNET:
                    break;
                default:
                    break;
            }
        }

        private DatabaseCredentials GetDatabaseCredentialsFromSettings()
        {
            DatabaseCredentials databaseCredentials = new DatabaseCredentials()
            {
                Database = this.DefaultSettings["Initial Catalog"],
                Password = this.DefaultSettings["Password"],
                Server = this.DefaultSettings["Data Source"],
                UserId = this.DefaultSettings["User Id"]
            };

            return databaseCredentials;
        }


        /// <summary>
        /// Run the command
        /// </summary>
        /// <param name="args">Excepts an boolean, indication verbose mode value</param>
        public void Run(params object[] args)
        {
            bool verbose = (bool)args[0];

            SQLServerGatherer sqlGatherer = null;
            try
            {
                string connectionString = this.DefaultSettings["DefaultConnectionString"];
                if (!string.IsNullOrEmpty(connectionString))
                    sqlGatherer = new SQLServerGatherer(connectionString);
                else
                {
                    try
                    {
                        sqlGatherer = new SQLServerGatherer(GetDatabaseCredentialsFromSettings());
                    }
                    catch
                    {
                        this.View.DisplayMessage("Could not get database information from settings.eixox.");
                        return;
                    }

                }
            }
            catch (KeyNotFoundException)
            {
                this.View.DisplayMessage("DefaultConnectionString key not found in settings.eixox file.");
                this.View.DisplayMessage("Trying to get database credential information...");

                try
                {
                    sqlGatherer = new SQLServerGatherer(GetDatabaseCredentialsFromSettings());
                }
                catch
                {
                    this.View.DisplayMessage("Could not get database information from settings.eixox.");
                    return;
                }
            }

            this.View.DisplayMessage("\n -- Running Command: Classes from database ---");
            DateTime tStart = DateTime.Now;

            if (verbose)
                this.View.DisplayMessage("Fetching database information...");

            List<GenericDatabaseTable> tables = sqlGatherer.GetTables().ToList();

            if (tables.Count <= 0)
            {
                if (verbose)
                    this.View.DisplayMessage("No tables found on this database");

                return;
            }

            if (verbose)
                this.View.DisplayMessage("Found " + tables.Count + " tables (" + DateTime.Now.Subtract(tStart).TotalSeconds + " seconds). Fetching columns information...");

            tStart = DateTime.Now;

            foreach (GenericDatabaseTable table in tables)
            {
                table.Columns = sqlGatherer.GetColumns(table.Name).ToList();
                if (verbose)
                    this.View.DisplayMessage(" [" + table.Name + "]: " + table.Columns.Count + " columns found");
            }

            if (verbose)
            {
                this.View.DisplayMessage("Found all columns (" + DateTime.Now.Subtract(tStart).TotalSeconds + " seconds)");
                this.View.DisplayMessage("Generating classes...");
            }

            tStart = DateTime.Now;
            foreach (GenericDatabaseTable table in tables)
            {
                if (verbose)
                    this.View.DisplayMessage("Creating " + table.Name + " class (using " + Enum.GetName(typeof(ProgrammingLanguage), this.Language) + ")");

                this.ClassGenerator.CreateClass(table, this.Language).Save(this.ClassGenerator._Directory);
            }

            if (verbose)
                this.View.DisplayMessage("Created " + tables.Count + " files (" + DateTime.Now.Subtract(tStart).TotalSeconds + " seconds)");

            this.View.DisplayMessage("Command run succesfully");
            this.View.DisplayMessage("-----------------------------");
        }

        public override string Directory
        {
            get { return this._Directory; }
        }
    }
}
