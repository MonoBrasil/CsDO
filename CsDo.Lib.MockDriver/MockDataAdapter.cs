using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;

namespace CsDO.Lib.MockDriver
{
    public class MockDataAdapter : DbDataAdapter
    {
        #region Private Vars

        private DbDataAdapter _dbDataAdapter = ProviderFactory.Instance.CreateDataAdapter();

        #endregion

        #region Constructors

        public MockDataAdapter()
        {
            _dbDataAdapter.FillError += new FillErrorEventHandler(_dbDataAdapter_FillError);
        }

        public MockDataAdapter(DbCommand command) : this()
        {
            SelectCommand = command;
        }

        #endregion

        #region Event Handlers

        private void _dbDataAdapter_FillError(object sender, FillErrorEventArgs e)
        {
            base.OnFillError(e);
        }

        #endregion

        public override int Fill(DataSet dataSet)
        {
            return base.Fill(dataSet);
        }
    }
}
