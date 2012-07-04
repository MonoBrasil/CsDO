/*
 * Created by Alexandre Rocha Lima e Marcondes
 * User: Administrator
 * Date: 13/08/2005
 * Time: 16:13
 * 
 * Description: An SQL Builder, Object Interface to Database Tables
 * Its based on DataObjects from php PEAR
 *  1. Builds SQL statements based on the objects vars and the builder methods.
 *  2. acts as a datastore for a table row.
 *  The core class is designed to be extended for each of your tables so that you put the
 *  data logic inside the data classes.
 *  included is a Generator to make your configuration files and your base classes.
 * 
 * CSharp DataObject 
 * Copyright (c) 2005, Alessandro de Oliveira Binhara
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are 
 * permitted provided that the following conditions are met:
 * 
 * - Redistributions of source code must retain the above copyright notice, this list 
 * of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above copyright notice, this list
 * of conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 *
 * - Neither the name of the <ORGANIZATION> nor the names of its contributors may be used to 
 * endorse or promote products derived from this software without specific prior written 
 * permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS &AS IS& AND ANY EXPRESS 
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
 * AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER 
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Reflection;
using System.Data;
using System.Collections;
using NUnit.Framework;
using CsDO.Lib;
using CsDO.Lib.MockDriver;

namespace CsDO.Tests {

	[TestFixture]
	public class UtilTests {

		[Test, ExpectedException(typeof(TypeInitializationException))]
		public void SingletonCreation() {
			new Singleton();
		}

		[Test]
		public void SingletonInstances() {
			Singleton inst1 = Singleton.New();
			Singleton inst2 = Singleton.New();
			Singleton inst3 = Singleton.New();
			Assert.IsTrue(Singleton.References > 2, "Singleton references not working properly");
			Assert.IsTrue(inst1.Equals(inst2) && inst2.Equals(inst3),"Singleton instancing not working properly");
		}
	}

	[TestFixture]
	public class DataBaseTests {

		[TestFixtureSetUp]
		public void FixtureSetup() {
			Conf.Driver = new MockDriver();
            Conf.DataPooling = true;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown() {
			Conf.Driver = null;
		}

		[Test]
		public void ConfInstance() {
			Assert.IsTrue(Conf.Driver != null, "Conf setup class not working properly");
		}

		[Test]
		public void Exec() {
			DataBase db = DataBase.New();

			Assert.AreEqual(0, db.Exec("CREATE TABLE teste (ID INT, Nome VARCHAR(12))"), "Exec not working properly");
			//Assert.AreEqual(1, db.Exec("INSERT INTO teste (Nome) VALUES ('teste') WHERE ID=1"), "Exec not working properly");
			Assert.AreEqual(1, db.Exec("INSERT INTO teste (Nome) VALUES ('teste')"), "Exec not working properly");
			Assert.AreEqual(0, db.Exec("SELECT * FROM teste WHERE ID=1"), "Exec not working properly");
			Assert.AreEqual(0, db.Exec("SELECT * FROM teste"), "Exec not working properly");
			Assert.AreEqual(0, db.Exec("UPDATE teste SET Nome='teste' WHERE ID=1"), "Exec not working properly");
			Assert.AreEqual(1, db.Exec("UPDATE teste SET Nome='teste', ID=1"), "Exec not working properly");
			Assert.AreEqual(1, db.Exec("DELETE FROM teste WHERE ID=1"), "Exec not working properly");
			Assert.AreEqual(0, db.Exec("DELETE FROM teste"), "Exec not working properly");
			Assert.AreEqual(0, db.Exec("DROP TABLE teste"), "Exec not working properly");
		}
	}

    #region Objects

    public class Sequence
    {
        private int sequence = 0;
        public int ID
        {
            get
            {
                sequence++;
                return sequence;
            }
        }
        public int seq
        {
            get
            {
                return sequence;
            }
        }
        public int Increment(DataObject table)
        {
            return ID;
        }
    }

    public class TesteObj : DataObject
    {
        private int _id;
        [Column("Cod"), PrimaryKey()]
        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        private string _nome;
        public string Nome
        {
            get
            {
                return _nome;
            }
            set
            {
                _nome = value;
            }
        }

        private int _idade;
        public int Idade
        {
            get
            {
                return _idade;
            }
            set
            {
                _idade = value;
            }
        }

        private double _peso;
        [Column("PesoKg")]
        public double Peso
        {
            get
            {
                return _peso;
            }
            set
            {
                _peso = value;
            }
        }

        private DateTime _aniversario;
        public DateTime Aniversario
        {
            get
            {
                return _aniversario;
            }
            set
            {
                _aniversario = value;
            }
        }

        private TesteObj3 _teste;
        [Column("Teste1")]
        public TesteObj3 TesteObj3
        {
            get
            {
                return _teste;
            }
            set
            {
                _teste = value;
            }
        }

        private bool _ativo;
        public bool Ativo
        {
            get { return _ativo; }
            set { _ativo = value; }
        }

        #region Proxies
        public void SetField(string col, object val)
        {
            setField(col, val);
        }
        public string GetFields()
        {
            return Fields;
        }
        public string GetActiveFields()
        {
            return ActiveFields;
        }
        public string GetWhere()
        {
            return Where;
        }
        public void SetWhere(string value)
        {
            Where = value;
        }
        public string GetLimit()
        {
            return Limit;
        }
        public void SetLimit(string value)
        {
            Limit = value;
        }
        public string GetOrderBy()
        {
            return OrderBy;
        }
        public void SetOrderBy(string value)
        {
            OrderBy = value;
        }
        public string GetGroupBy()
        {
            return GroupBy;
        }
        public void SetGroupBy(string value)
        {
            GroupBy = value;
        }
        public IList GetPrimaryKeys()
        {
            return PrimaryKeys;
        }
        private Sequence sequence = new Sequence();
        #endregion

        public TesteObj()
        {
            autoIncrement += new AutoIncrement(sequence.Increment);
        }
    }

    public class TesteObj3 : DataObject
    {
        private int _id;
        [Column("Cod"), PrimaryKey()]
        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        private string _nome;
        public string Nome
        {
            get
            {
                return _nome;
            }
            set
            {
                _nome = value;
            }
        }

        private int _idade;
        public int Idade
        {
            get
            {
                return _idade;
            }
            set
            {
                _idade = value;
            }
        }

        private double _peso;
        [Column("PesoKg")]
        public double Peso
        {
            get
            {
                return _peso;
            }
            set
            {
                _peso = value;
            }
        }
    }

    [Table("TesteObject")]
    public class TesteObj2 : TesteObj { }
    
    #endregion
	
	[TestFixture]
	public class DataObjectTests {

		[TestFixtureSetUp]
		public void FixtureSetup() {
			Conf.Driver = new MockDriver();
            Conf.DataPooling = true;

			((MockDriver) Conf.Driver).addTable("TesteObj");
			((MockDriver) Conf.Driver).addColumn("TesteObj", "Cod", typeof(Int32), true, false);
			((MockDriver) Conf.Driver).addColumn("TesteObj", "Idade", typeof(Int32));
			((MockDriver) Conf.Driver).addColumn("TesteObj", "Nome", typeof(string));
			((MockDriver) Conf.Driver).addColumn("TesteObj", "PesoKg", typeof(Double));
			((MockDriver) Conf.Driver).addColumn("TesteObj", "Teste1", typeof(Int32));
			((MockDriver) Conf.Driver).addColumn("TesteObj", "Ativo", typeof(bool));
            ((MockDriver)Conf.Driver).addColumn("TesteObj", "Aniversario", typeof(DateTime));
			
			DataRow data = ((MockDriver) Conf.Driver).newRow("TesteObj");
			data["Cod"] = 1;
			data["Idade"] = 18;
			data["Nome"] = "teste1";
			data["PesoKg"] = 60.5;
			data["Teste1"] = 0;
			data["Ativo"] = true;
            data["Aniversario"] = DateTime.Now;
			((MockDriver) Conf.Driver).addRow("TesteObj", data);
			
			data = ((MockDriver) Conf.Driver).newRow("TesteObj");
			data["Cod"] = 2;
			data["Idade"] = 20;
			data["Nome"] = "teste2";
			data["PesoKg"] = 80.5;
			data["Teste1"] = 0;
			data["Ativo"] = true;
            data["Aniversario"] = DateTime.Now.AddMonths(-2);
			((MockDriver) Conf.Driver).addRow("TesteObj", data);
			
			data = ((MockDriver) Conf.Driver).newRow("TesteObj");
			data["Cod"] = 3;
			data["Idade"] = 22;
			data["Nome"] = "teste3";
			data["PesoKg"] = 50.5;
			data["Teste1"] = 0;
			data["Ativo"] = true;
            data["Aniversario"] = DateTime.Now.AddMonths(2);
			((MockDriver) Conf.Driver).addRow("TesteObj", data);

			data = ((MockDriver) Conf.Driver).newRow("TesteObj");
			data["Cod"] = 4;
			data["Idade"] = 28;
			data["Nome"] = "teste4";
			data["PesoKg"] = 70.5;
			data["Teste1"] = 0;
			data["Ativo"] = true;
            data["Aniversario"] = DateTime.Now.AddDays(2);
			((MockDriver) Conf.Driver).addRow("TesteObj", data);	
			
			((MockDriver) Conf.Driver).addTable("TesteObj3");
			((MockDriver) Conf.Driver).addColumn("TesteObj3", "Cod", typeof(Int32), true, false);
			((MockDriver) Conf.Driver).addColumn("TesteObj3", "Idade", typeof(Int32));
			((MockDriver) Conf.Driver).addColumn("TesteObj3", "Nome", typeof(string));
			((MockDriver) Conf.Driver).addColumn("TesteObj3", "PesoKg", typeof(Double));
			
			data = ((MockDriver) Conf.Driver).newRow("TesteObj3");
			data["Cod"] = 5;
			data["Idade"] = 50;
			data["Nome"] = "teste5";
			data["PesoKg"] = 100.5;
			((MockDriver) Conf.Driver).addRow("TesteObj3", data);			
		}

		[TestFixtureTearDown]
		public void FixtureTearDown() {
			Conf.Driver = null;
		}

		[Test]
		public void Inheritance() {
			TesteObj2 obj = new TesteObj2();
			obj.ID = 1;
			obj.Nome = "teste";
			obj.Idade = 18;
			obj.Peso = 60.5;
			obj.Ativo = true;
			obj.insert();

            Assert.AreEqual("INSERT INTO TesteObject (Cod,Nome,Idade,PesoKg,Ativo) Values (1,'teste',18,60.5,'T')", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot insert into table");
			Assert.IsTrue(obj.find(), "DataObject find");
            Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObject WHERE (Cod = 1) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot select from table");
			Assert.IsTrue(obj.retrieve("Nome", "teste"), "DataObject retrieve");
            Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObject WHERE Nome LIKE 'teste'", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");
		}
		
		[Test]
		public void Insert() {
			TesteObj obj = new TesteObj();
			obj.ID = 1;
			obj.Nome = "teste";
			obj.Idade = 18;
			obj.Peso = 60.5;
			obj.Ativo = true;
	
			Assert.AreEqual(true, obj.insert(), "DataObject cannot insert into table");
            Assert.AreEqual("INSERT INTO TesteObj (Cod,Nome,Idade,PesoKg,Ativo) Values (1,'teste',18,60.5,'T')", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot insert into table");
			Assert.AreEqual(1, obj.ID, "DataObject auto increment not working");

			obj = new TesteObj();
			obj.ID = 1;
			obj.Peso = 60.5;
			obj.insert();
			
			Assert.AreEqual(true, obj.insert(), "DataObject cannot insert into table");
            Assert.AreEqual("INSERT INTO TesteObj (Cod,PesoKg,Ativo) Values (2,60.5,'F')", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot insert into table");			
			Assert.AreEqual(2, obj.ID, "DataObject auto increment not working");
		}
	
		[Test]
		public void FindFetch() {
			TesteObj obj = new TesteObj();
            obj.SetDebug(true);
			obj.ID = 1;
			obj.Nome = "teste";
			
			Assert.IsTrue(obj.find(), "DataObject find");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE (Cod = 1) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot select from table");
			Assert.IsTrue(obj.fetch(), "DataObject fetch from DataReader");

			obj = new TesteObj();
			obj.Nome = "teste";
			obj.Idade = 18;
			obj.Peso = 60.5;
			obj.Ativo = true;
			
			Assert.IsTrue(obj.find(), "DataObject find");
            Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE (Nome LIKE 'teste') AND (Idade = 18) AND (PesoKg = 60.5) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot select from table");
			Assert.IsTrue(obj.fetch(), "DataObject find");
            Assert.AreEqual(1, obj.ID, "DataPool not working");

            obj = new TesteObj();
            obj.SetDebug(true);
            obj.ID = 1;

            Assert.IsTrue(obj.find(), "DataObject find");
            Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE (Cod = 1) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot select from table");
            Assert.IsTrue(obj.fetch(), "DataObject fetch from DataReader");
            Assert.AreEqual(60.5, obj.Peso, "DataPool not working");
        }

		[Test]
		public void SetField() {
			TesteObj obj = new TesteObj();
			obj.ID = 2;
			obj.Nome = "teste 2";
			obj.SetField("Cod", 1);
			obj.SetField("Nome", "teste");
			obj.SetField("PesoKg", 60.5);
			obj.SetField("Idade", 18);
			obj.SetField("Ativo", true);
			
			Assert.AreEqual(1, obj.ID, "DataObject cannot set Field 'ID'");
			Assert.AreEqual("teste", obj.Nome, "DataObject cannot set Field 'Nome'");
			Assert.AreEqual(60.5, obj.Peso, "DataObject cannot set Field 'Peso'");
			Assert.AreEqual(18, obj.Idade, "DataObject cannot set Field 'Idade'");
			Assert.IsTrue(obj.Ativo, "DataObject cannot set Field 'Ativo'");
			obj.SetField("ID", 2);
			Assert.AreEqual(2, obj.ID, "DataObject cannot set Field 'ID'");
			obj.SetField("Peso", 61.5);
			Assert.AreEqual(61.5, obj.Peso, "DataObject cannot set Field 'Peso'");
			obj.SetField("Ativo", false);
			Assert.IsFalse(obj.Ativo, "DataObject cannot set Field 'Ativo'");
			obj.SetField("Ativo", "T");
			Assert.IsTrue(obj.Ativo, "DataObject cannot set Field 'Ativo'");
			obj.SetField("Ativo", "F");
			Assert.IsFalse(obj.Ativo, "DataObject cannot set Field 'Ativo'");
		}

		[Test, ExpectedException(typeof(CsDOException))]
		public void SetFieldError() {
			TesteObj obj = new TesteObj();
			obj.ID = 1;
			obj.Nome = "teste";
			obj.SetField("Codigo", 2);
		}
	
		[Test]
		public void GetFields() {
			TesteObj obj = new TesteObj();

            Assert.AreEqual("Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo", obj.GetFields(), "DataObject getFields");
		}

		[Test]
		public void GetActiveFields() {
			TesteObj obj = new TesteObj();

            Assert.AreEqual("Ativo", obj.GetActiveFields(), "DataObject getActiveFields");
		}
				
		[Test]
		public void Retrieve() {
			TesteObj obj = new TesteObj();
			
			Assert.IsTrue(obj.retrieve("Nome", "teste1"), "DataObject retrieve");
            Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE Nome LIKE 'teste1'", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");
			Assert.IsTrue(obj.fetch(), "DataObject fetch");
			Assert.AreEqual(1, obj.ID, "DataObject retrieve #1");
			Assert.AreEqual(18, obj.Idade, "DataObject retrieve");
			Assert.AreEqual(60.5, obj.Peso, "DataObject retrieve");
			Assert.IsNull(obj.TesteObj3, "DataObject retrieve");
			Assert.IsTrue(obj.Ativo, "DataObject retrieve");
			Assert.AreEqual("teste1", obj.Nome, "DataObject retrieve");

			obj = new TesteObj();

			Assert.IsTrue(obj.retrieve("Cod", 1), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE Cod = 1", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");
			Assert.IsTrue(obj.fetch(), "DataObject fetch");
			Assert.AreEqual(1, obj.ID, "DataObject retrieve #2");
			Assert.AreEqual(18, obj.Idade, "DataObject retrieve");
			Assert.AreEqual(60.5, obj.Peso, "DataObject retrieve");
			Assert.IsNull(obj.TesteObj3, "DataObject retrieve");
			Assert.IsTrue(obj.Ativo, "DataObject retrieve");
			Assert.AreEqual("teste1", obj.Nome, "DataObject retrieve");

			obj = new TesteObj();

			Assert.IsTrue(obj.retrieve("ID", 1), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE Cod = 1", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");
			Assert.IsTrue(obj.fetch(), "DataObject fetch");
			Assert.AreEqual(1, obj.ID, "DataObject retrieve #3");
			Assert.AreEqual(18, obj.Idade, "DataObject retrieve");
			Assert.AreEqual(60.5, obj.Peso, "DataObject retrieve");
			Assert.IsNull(obj.TesteObj3, "DataObject retrieve");
			Assert.IsTrue(obj.Ativo, "DataObject retrieve");
			Assert.AreEqual("teste1", obj.Nome, "DataObject retrieve");

			obj = new TesteObj();

			Assert.IsTrue(obj.retrieve("Peso", 60.5), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE PesoKg = 60.5", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");			
			Assert.IsTrue(obj.fetch(), "DataObject fetch");
			Assert.AreEqual(1, obj.ID, "DataObject retrieve #4");
			Assert.AreEqual(18, obj.Idade, "DataObject retrieve");
			Assert.AreEqual(60.5, obj.Peso, "DataObject retrieve");
			Assert.IsNull(obj.TesteObj3, "DataObject retrieve");
			Assert.IsTrue(obj.Ativo, "DataObject retrieve");
			Assert.AreEqual("teste1", obj.Nome, "DataObject retrieve");

			obj = new TesteObj();

			Assert.IsTrue(obj.retrieve("PesoKg", 60.5), "DataObject retrieve");
			Assert.IsTrue(obj.fetch(), "DataObject fetch");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE PesoKg = 60.5", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");			
			Assert.AreEqual(1, obj.ID, "DataObject retrieve #5");
			Assert.AreEqual(18, obj.Idade, "DataObject retrieve");
			Assert.AreEqual(60.5, obj.Peso, "DataObject retrieve");
			Assert.IsNull(obj.TesteObj3, "DataObject retrieve");
			Assert.IsTrue(obj.Ativo, "DataObject retrieve");
			Assert.AreEqual("teste1", obj.Nome, "DataObject retrieve");

			obj = new TesteObj();

			Assert.IsTrue(obj.retrieve("Idade", 18), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE Idade = 18", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");						
			Assert.IsTrue(obj.fetch(), "DataObject fetch");
			Assert.AreEqual(1, obj.ID, "DataObject retrieve #6");
			Assert.AreEqual(18, obj.Idade, "DataObject retrieve");
			Assert.AreEqual(60.5, obj.Peso, "DataObject retrieve");
			Assert.IsNull(obj.TesteObj3, "DataObject retrieve");
			Assert.IsTrue(obj.Ativo, "DataObject retrieve");
			Assert.AreEqual("teste1", obj.Nome, "DataObject retrieve");
		}
		
		[Test]
		public void PrimaryKey() {
			TesteObj obj = new TesteObj();
			
			Assert.IsNotNull(obj.GetPrimaryKeys(), "PrimaryKey is null");
			Assert.AreEqual(1, obj.GetPrimaryKeys().Count, "PrimaryKey size is not 1");
			Assert.AreEqual("ID", ((PropertyInfo) obj.GetPrimaryKeys()[0]).Name, "PrimaryKey not found");
			Assert.AreEqual("Cod", ((Column)((PropertyInfo) obj.GetPrimaryKeys()[0]).GetCustomAttributes(typeof(Column), true)[0]).Name, "PrimaryKey properties not found");
		}
		
		[Test]
		public void PrimaryKeyConstructor() {
            Conf.DataPool.Clear();
			TesteObj obj = new TesteObj();
			obj.Get(1);

            Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE (Cod = 1) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot find object from Primary Key");
			Assert.AreEqual(1, obj.ID, "DataObject cannot set Field 'ID' #1");
			Assert.AreEqual("teste1", obj.Nome, "DataObject cannot set Field 'Nome'");
			Assert.AreEqual(60.5, obj.Peso, "DataObject cannot set Field 'Peso'");
			Assert.AreEqual(18, obj.Idade, "DataObject cannot set Field 'Idade'");
			Assert.IsNull(obj.TesteObj3, "DataObject cannot set Field 'TesteObj3'");
			Assert.IsTrue(obj.Ativo, "DataObject retrieve");
			
			obj = new TesteObj();
			obj.Get(5);
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE (Cod = 5) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot find object from Primary Key");
			Assert.AreEqual(1, obj.ID, "DataObject cannot set Field 'ID' #2");
		}	
		
		[Test]
		public void Delete() {
			TesteObj obj = new TesteObj();
			obj.ID = 1;

			Assert.AreEqual(true, obj.delete(), "DataObject cannot delete from table");
			Assert.AreEqual("DELETE FROM TesteObj WHERE (Cod = 1) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot delete from table");
			
			obj.ID = 2;
			obj.Nome = "teste";
			
			Assert.AreEqual(true, obj.delete(), "DataObject cannot delete from table");
			Assert.AreEqual("DELETE FROM TesteObj WHERE (Cod = 2) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot delete from table");			
		}		
		
		[Test]
		public void Update() {
			TesteObj obj = new TesteObj();
			obj.ID = 1;
			obj.Nome = "teste2";
			obj.Idade = 22;
			obj.Peso = 70.5;
			obj.Ativo = true;
			
			Assert.AreEqual(true, obj.update(), "DataObject cannot update table");
            Assert.AreEqual("UPDATE TesteObj SET Nome='teste2',Idade=22,PesoKg=70.5,Ativo='T' WHERE (Cod = 1) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot update table");

			obj = new TesteObj();
			obj.ID = 2;
			obj.Peso = 71.5;
			obj.Nome = "teste3";
			
			Assert.AreEqual(true, obj.update(), "DataObject cannot update table");
            Assert.AreEqual("UPDATE TesteObj SET Nome='teste3',PesoKg=71.5,Ativo='F' WHERE (Cod = 2) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot update table");		
		}
		
		[Test]
		public void RetrieveModifiers() {
			TesteObj obj = new TesteObj();
			
			obj.SetWhere("Idade > 18");		
			Assert.IsTrue(obj.retrieve("Nome", "teste"), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE Nome LIKE 'teste' AND Idade > 18", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");

			obj.SetOrderBy("Nome,Idade");		
			Assert.IsTrue(obj.retrieve("PesoKg", 50), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE PesoKg = 50 AND Idade > 18 ORDER BY Nome,Idade", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");

			obj.SetGroupBy("Idade,Nome,PesoKg,Teste1,Cod");		
			Assert.IsTrue(obj.retrieve("PesoKg", 50), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE PesoKg = 50 AND Idade > 18 GROUP BY Idade,Nome,PesoKg,Teste1,Cod ORDER BY Nome,Idade", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");

			obj.SetLimit("10");		
			Assert.IsTrue(obj.retrieve("PesoKg", 50), "DataObject retrieve");
			Assert.AreEqual("SELECT TOP 10 Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE PesoKg = 50 AND Idade > 18 GROUP BY Idade,Nome,PesoKg,Teste1,Cod ORDER BY Nome,Idade", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");
			
			obj.SetWhere(null);		
			Assert.IsTrue(obj.retrieve("PesoKg", 50), "DataObject retrieve");
			Assert.AreEqual("SELECT TOP 10 Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE PesoKg = 50 GROUP BY Idade,Nome,PesoKg,Teste1,Cod ORDER BY Nome,Idade", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");

			obj.SetLimit(null);		
			obj.SetGroupBy("");				
	
			Assert.IsTrue(obj.retrieve("PesoKg", 50), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE PesoKg = 50 ORDER BY Nome,Idade", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");

			obj.SetOrderBy("");
			Assert.IsTrue(obj.retrieve("PesoKg", 50), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE PesoKg = 50", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");			
		}

        [Test]
        public void CompareOperators()
        {
            TesteObj obj1 = new TesteObj();
            obj1.ID = 1;
            obj1.Nome = "Obj1";

            TesteObj obj2 = new TesteObj();
            obj2.ID = 1;
            obj2.Nome = "Obj1";

            TesteObj3 obj3 = new TesteObj3();
            obj3.ID = 1;
            obj3.Nome = "Obj1";


            Assert.AreEqual("CsDO.Tests.TesteObj!1".GetHashCode(), obj1.GetHashCode(), "GetHashCode() #1 failing");
            Assert.AreEqual("CsDO.Tests.TesteObj!1".GetHashCode(), obj2.GetHashCode(), "GetHashCode() #2 failing");
            Assert.AreEqual("CsDO.Tests.TesteObj3!1".GetHashCode(), obj3.GetHashCode(), "GetHashCode() #3 failing");
            Assert.IsTrue(obj1.Equals(obj2), "Equals operator #1 failing");
            Assert.IsTrue(obj2.Equals(obj1), "Equals operator #2 failing");
            Assert.IsFalse(obj1.Equals(obj3), "Equals operator #3 failing"); 
            Assert.IsTrue(obj1 == obj2, "== operator #1 failing");
            Assert.IsTrue(obj2 == obj1, "== operator #2 failing");
            Assert.IsFalse(obj1 == obj3, "== operator #3 failing");
            Assert.IsFalse(obj1 != obj2, "!= operator #1 failing");
            Assert.IsFalse(obj2 != obj1, "!= operator #2 failing");
            Assert.IsTrue(obj1 != obj3, "!= operator #3 failing");
        }
		
						
	}	
}
