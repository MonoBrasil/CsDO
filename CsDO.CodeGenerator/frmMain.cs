using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using CsDO.Lib;
using System.Reflection;

namespace CsDO.CodeGenerator
{
    public partial class frmMain : Form
    {
        private IDataBase driver = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            cbxDriver.Items.Clear();

            foreach (FieldInfo item in typeof(Config.DBMS).GetFields())
                if (item.IsStatic)
                    cbxDriver.Items.Add(item.Name);

            cbxDriver.SelectedIndex = 0;

            CompilerInfo[] compilers = CodeDomProvider.GetAllCompilerInfo();
            cbxLanguage.Items.AddRange(compilers[0].GetLanguages());
            cbxLanguage.SelectedIndex = 0;
        }

        private void btnFolderSelect_Click(object sender, EventArgs e)
        {
            DialogResult result = dlgFolder.ShowDialog();

            if (!txtDestinationPath.Text.Trim().Equals(String.Empty))
            {
                dlgFolder.SelectedPath = txtDestinationPath.Text;
            }

            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                txtDestinationPath.Text = dlgFolder.SelectedPath;
            }
        }

        private void CreateConnection()
        {
            switch (cbxDriver.SelectedIndex)
            {
                case (int) Config.DBMS.PostgreSQL:
                    driver = new CsDO.Drivers.Npgsql.NpgsqlDriver(
                        Config.Server, Config.Port, Config.User,
                        Config.Database, Config.Password);
                    break;
                case (int) Config.DBMS.OleDB:
                    driver = new CsDO.Drivers.OleDb.OleDbDriver(
                        Config.Server, Config.Database, Config.User,
                        Config.Password);
                    break;
                case (int) Config.DBMS.MSSQLServer:
                    driver = new CsDO.Drivers.SqlServer.SqlServerDriver(
                        Config.Server, Config.User, Config.Database,
                        Config.Password);
                    break;
                default:
                    driver = null;
                    break;
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            string connectionString = Config.CreateDbConectionString((Config.DBMS) cbxDriver.SelectedIndex);

            CreateConnection();

            try
            {
                IDbConnection conn = driver.getConnection();
                lbxDatabase.Items.Clear();
                lbxDatabase.Items.AddRange(SchemaReader.ReadSchema(driver).ToArray());
                lbxDatabase.Sorted = true;
                driver.close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Connection Error");
            }
            
        }

        private void txtDBServer_TextChanged(object sender, EventArgs e)
        {
            Config.Server = txtDBServer.Text;
        }

        private void txtDBName_TextChanged(object sender, EventArgs e)
        {
            Config.Database = txtDBName.Text;
        }

        private void txtDBUserName_TextChanged(object sender, EventArgs e)
        {
            Config.User = txtDBUserName.Text;
        }

        private void txtDBPassword_TextChanged(object sender, EventArgs e)
        {
            Config.Password = txtDBPassword.Text;
        }

        private void txtDBPort_TextChanged(object sender, EventArgs e)
        {
            Config.Port = txtDBPort.Text;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            EntityGenerator eg = new EntityGenerator(txtNamespace.Text, driver);

            foreach (string name in lbxDatabase.CheckedItems)
                 eg.Run(name, null, txtDestinationPath.Text);
        }

        private void cbxDriver_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbxDriver.SelectedIndex)
            {
                case 1:
                    txtDBPort.Text = "5432";
                    txtDBUserName.Text = "postgres";
                    break;
                default:
                    break;
            }
        }

    }
}