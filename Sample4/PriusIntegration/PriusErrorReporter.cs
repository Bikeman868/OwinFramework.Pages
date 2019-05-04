using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Prius.Contracts.Interfaces.External;

namespace Sample4.PriusIntegration
{
    internal class PriusErrorReporter: IErrorReporter
    {
        void IErrorReporter.ReportError(Exception e, SqlCommand cmd, string subject, params object[] otherInfo)
        {
            Trace.WriteLine("Prius: " + e.Message);
        }

        void IErrorReporter.ReportError(Exception e, string subject, params object[] otherInfo)
        {
            Trace.WriteLine("Prius: " + e.Message);
        }

        void IDisposable.Dispose()
        {
        }
    }
}