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

            Progress.Minimum = 0;
            Progress.Maximum = lbxDatabase.CheckedItems.Count;
            Progress.Step = 1;
            Progress.AutoSize = false;
            Progress.Width = this.Width;
            Progress.Visible = true;
            foreach (ClassDefinition table in lbxDatabase.CheckedItems)
            {
                eg.Run(table, txtDestinationPath.Text);
                Progress.PerformStep();
            }
            Progress.Visible = false;
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

        private void miSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbxDatabase.Items.Count; i++)
                lbxDatabase.SetItemChecked(i, true);
        }

        private void miDeselectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbxDatabase.Items.Count; i++)
                lbxDatabase.SetItemChecked(i, false);
        }

        private void miSelectTable_Click(object sender, EventArgs e)
        {
            lbxDatabase.SetItemChecked(lbxDatabase.SelectedIndex, true);
        }

        private void miDeselectTable_Click(object sender, EventArgs e)
        {
            lbxDatabase.SetItemChecked(lbxDatabase.SelectedIndex, false);
        }

        private void cmsDatabase_Opening(object sender, CancelEventArgs e)
        {
            miSelectAll.Enabled = lbxDatabase.Items.Count > 0;
            miDeselectAll.Enabled = lbxDatabase.Items.Count > 0;
            miSelectTable.Enabled = lbxDatabase.Items.Count > 0 && lbxDatabase.SelectedIndex >= 0 && !lbxDatabase.GetItemChecked(lbxDatabase.SelectedIndex);
            miSelectTable.Visible = miSelectTable.Enabled;
            miDeselectTable.Enabled = lbxDatabase.Items.Count > 0 && lbxDatabase.SelectedIndex >= 0 && lbxDatabase.GetItemChecked(lbxDatabase.SelectedIndex);
            miDeselectTable.Visible = miDeselectTable.Enabled;
            miRenameTable.Enabled = lbxDatabase.Items.Count > 0 && lbxDatabase.SelectedIndex >= 0;
        }

        private void lbxDatabase_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int i = lbxDatabase.IndexFromPoint(e.X, e.Y);
                lbxDatabase.ClearSelected();
                lbxDatabase.SetSelected(i, true);
            }
        }

        private TextBox edit = null;
        private int index = -1;

        void edit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                lbxDatabase.Focus();
        }

        void edit_Leave(object sender, EventArgs e)
        {
            ClassDefinition table = ((ClassDefinition)lbxDatabase.Items[index]);
            table.Alias = !table.Table.Equals(edit.Text.Trim()) ? edit.Text.Trim() : null;
            lbxDatabase.Controls.Remove(edit);
            edit.Dispose();
        }

        private void lbxDatabase_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
        }

        private void miRenameTable_Click(object sender, EventArgs e)
        {
            int i = lbxDatabase.SelectedIndex;
            if (i > -1 && lbxDatabase.GetSelected(i))
            {
                Rectangle rect = lbxDatabase.GetItemRectangle(i);
                edit = new TextBox();
                edit.BorderStyle = BorderStyle.FixedSingle;
                edit.SetBounds(rect.Left + 15, rect.Top,
                    rect.Width - 15, rect.Height - 4);
                edit.Text = lbxDatabase.Items[i].ToString();
                edit.Leave += new EventHandler(edit_Leave);
                edit.KeyPress += new KeyPressEventHandler(edit_KeyPress);
                index = i;
                lbxDatabase.Controls.Add(edit);
            }
        }
    }
}