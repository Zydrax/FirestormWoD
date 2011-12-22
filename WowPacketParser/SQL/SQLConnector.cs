﻿using System;
using System.Data;
using MySql.Data.MySqlClient;
using WowPacketParser.Misc;

namespace WowPacketParser.SQL
{
    public static class SQLConnector
    {
        [ThreadStatic]
        private static MySqlConnection _conn;

        public static bool Enabled = Settings.GetBoolean("DBEnabled", false);

        public static void Connect()
        {
            if (!Enabled)
            {
                Console.WriteLine("DB queries are disabled. Will not connect.");
                return;
            }

            Console.WriteLine("Connecting to MySQL server: " + ConnectionString.Replace("Password=" + Settings.GetString("Password", "") + ";", string.Empty)); // Do not print password
            _conn = new MySqlConnection(ConnectionString);

            try
            {
                _conn.Open();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static bool Connected()
        {
            return _conn.State == ConnectionState.Open;
        }

        public static void Disconnect()
        {
            if (_conn != null)
                _conn.Close();
        }

        public static MySqlDataReader ExecuteQuery(string input)
        {
            var command = new MySqlCommand(input, _conn);

            try
            {
                return command.ExecuteReader();
            }
            catch (Exception e)
            {
                // Something wrong happened, disabling everything MySQL/DB related
                Enabled = false;
                Console.WriteLine(e.Message + " at query \"" + input + "\"");
                Disconnect();
            }

            return null;
        }

        private static string ConnectionString
        {
            get
            {
                var server = Settings.GetString("Server", "localhost");
                var protocol = String.Empty;
                var portOrPipe = "Port";
                if (server == ".")
                {
                    server = "localhost";
                    portOrPipe = "Pipe";
                    protocol = "ConnectionProtocol=Pipe;";
                }

                return String.Format("Server={0};{1}={2};Username={3};Password={4};Database={5};CharSet={6};ConnectionTimeout=5;{7}",
                    server,
                    portOrPipe,
                    Settings.GetString("Port", "3306"),
                    Settings.GetString("Username", "root"),
                    Settings.GetString("Password", ""),
                    Settings.GetString("Database", "WPP"),
                    Settings.GetString("CharacterSet", "utf8"),
                    protocol);
            }
        }
    }
}
