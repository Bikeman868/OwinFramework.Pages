using System;
using Prius.Contracts.Interfaces.External;
using System.Diagnostics;
using Prius.Contracts.Interfaces.Commands;
using System.Collections.Generic;

namespace Sample5.PriusIntegration
{
    public class PriusTraceWriterFactory : ITraceWriterFactory
    {
        ITraceWriter ITraceWriterFactory.Create(string repositoryName)
        {
            return new TraceWriter();
        }

        private class TraceWriter : ITraceWriter2
        {
            private DateTime _start;
            private List<IParameter> _parameters;
            private bool _firstLine = true;

            private string _cluster;
            private string _database;
            private string _procedure;

            public TraceWriter()
            {
                _start = DateTime.UtcNow;
                _parameters = new List<IParameter>();
            }

            void ITraceWriter.SetCluster(string clusterName)
            {
                _cluster = clusterName;
            }

            void ITraceWriter.SetDatabase(string databaseName)
            {
                _database = databaseName;
            }

            void ITraceWriter2.SetParameter(IParameter parameter)
            {
                _parameters.Add(parameter);
            }

            void ITraceWriter.SetProcedure(string storedProcedureName)
            {
                _procedure = storedProcedureName;
            }

            void ITraceWriter.WriteLine(string message)
            {
                if (_firstLine)
                {
                    Trace.WriteLine("Prius: Executing stored procedure " + _procedure + " on " + _database + " database");

                    foreach (var parameter in _parameters)
                        Trace.WriteLine("Prius: Parameter " + parameter.Name + "[" + parameter.Type.Name + "] = "+ (parameter.Value == null ? "NULL" : parameter.Value.ToString()));

                    _firstLine = false;
                }

                Trace.WriteLine("Prius: " + (DateTime.UtcNow - _start).TotalSeconds.ToString("n1") + "s - " + message);
            }
        }
    }
}