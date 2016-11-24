/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* Coin Exchange is a high performance exchange system specialized for
* Crypto currency trading. It has different general purpose uses such as
* independent deposit and withdrawal channels for Bitcoin and Litecoin,
* but can also act as a standalone exchange that can be used with
* different asset classes.
* Coin Exchange uses state of the art technologies such as ASP.NET REST API,
* AngularJS and NUnit. It also uses design patterns for complex event
* processing and handling of thousands of transactions per second, such as
* Domain Driven Designing, Disruptor Pattern and CQRS With Event Sourcing.
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.IO;
using MySql.Data.MySqlClient;

namespace CoinExchange.Common.Tests
{
    /// <summary>
    /// Database utility class to run script
    /// </summary>
    public class DatabaseUtility
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _connectionString;
        private MySqlConnection _mySqlConnection;
        private string _filePath = Path.GetFullPath(@"~\..\..\..\..\..\Data\MySql\");
        public DatabaseUtility(string connectionString)
        {
            _connectionString = connectionString;
            _mySqlConnection=new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// Run create sql script
        /// </summary>
        public void Create()
        {
            string script = File.ReadAllText(_filePath+"create.sql");
            ExecuteScript(script);
        }

        /// <summary>
        /// Run drop sql script
        /// </summary>
        public void Drop()
        {
            string script = File.ReadAllText(_filePath+"drop.sql");
            ExecuteScript(script);
        }

        /// <summary>
        /// Populate the database with master data.
        /// </summary>
        public void Populate()
        {
            string script = File.ReadAllText(_filePath+"populate.sql");
            ExecuteScript(script);
        }

        /// <summary>
        /// Execute sql script
        /// </summary>
        private void ExecuteScript(string script)
        {
            try
            {
                MySqlCommand command = new MySqlCommand(script, _mySqlConnection);
                _mySqlConnection.Open();
                command.ExecuteNonQuery();
                _mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("Execute Script Exception:",exception);
                }
            }
            finally
            {
                _mySqlConnection.Close();
            }
        }
    }
}
