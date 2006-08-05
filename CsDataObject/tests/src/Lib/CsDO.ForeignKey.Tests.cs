/*
 * Created by Alexandre Rocha Lima e Marcondes
 * User: Administrator
 * Date: 27/09/2005
 * Time: 15:49
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
using NUnit.Framework;
using CsDO.Lib;

namespace CsDO.Tests {

	public class TesteObj4 : DataObject {
		private int _id;
		[Column("Cod")]
		public int ID {
			get {
				return _id;
			}
			set {
				_id = value;
			}
		}
		
		private string _nome;
		public string Nome {
			get {
				return _nome;
			}
			set {
				_nome = value;
			}
		}

		private int _idade;
		public int Idade {
			get {
				return _idade;
			}
			set {
				_idade = value;
			}
		}
		
		private double _peso;
		[Column("PesoKg")]
		public double Peso {
			get {
				return _peso;
			}
			set {
				_peso = value;
			}
		}
		
		private TesteObj _teste;
		[Column("Teste1")]
		public TesteObj TesteObj3 {
			get {
				return _teste;
			}
			set {
				_teste = value;
			}
		}			
		
		public void SetField(string col, object val)
		{
			setField(col, val);
		}
		
		public bool Retrieve(string col, object val)
		{
			return retrieve(col, val);
		}		

		public string GetFields()
		{
			return Fields;
		}
		
		public string GetActiveFields()
		{
			return ActiveFields;
		}
	}	
	
	[TestFixture]
	public class ForeignKeyTests {

		[TestFixtureSetUp]
		public void FixtureSetup() {
			Conf.Driver = new MockDriver();

			((MockDriver) Conf.Driver).addTable("TesteObj");
			((MockDriver) Conf.Driver).addColumn("TesteObj", "Cod", typeof(Int32), true, false);
			((MockDriver) Conf.Driver).addColumn("TesteObj", "Idade", typeof(Int32));
			((MockDriver) Conf.Driver).addColumn("TesteObj", "Nome", typeof(string));
			((MockDriver) Conf.Driver).addColumn("TesteObj", "PesoKg", typeof(Double));
			((MockDriver) Conf.Driver).addColumn("TesteObj", "Teste1", typeof(Int32));
			
			DataRow data = ((MockDriver) Conf.Driver).newRow("TesteObj");
			data["Cod"] = 1;
			data["Idade"] = 18;
			data["Nome"] = "teste1";
			data["PesoKg"] = 60.5;
			data["Teste1"] = 5;
			((MockDriver) Conf.Driver).addRow("TesteObj", data);
			
			data = ((MockDriver) Conf.Driver).newRow("TesteObj");
			data["Cod"] = 2;
			data["Idade"] = 20;
			data["Nome"] = "teste2";
			data["PesoKg"] = 80.5;
			data["Teste1"] = 10;
			((MockDriver) Conf.Driver).addRow("TesteObj", data);
			
			data = ((MockDriver) Conf.Driver).newRow("TesteObj");
			data["Cod"] = 3;
			data["Idade"] = 22;
			data["Nome"] = "teste3";
			data["PesoKg"] = 50.5;
			data["Teste1"] = 15;
			((MockDriver) Conf.Driver).addRow("TesteObj", data);

			data = ((MockDriver) Conf.Driver).newRow("TesteObj");
			data["Cod"] = 4;
			data["Idade"] = 28;
			data["Nome"] = "teste4";
			data["PesoKg"] = 70.5;
			data["Teste1"] = 20;
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
			obj.TesteObj3 = new TesteObj3();
			obj.TesteObj3.ID = 5;
			obj.insert();
			
			Assert.AreEqual("INSERT INTO TesteObject (Cod,Nome,Idade,PesoKg,Teste1,Ativo) Values (1,'teste',18,60.5,5,'F')", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot insert into table");
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
			obj.TesteObj3 = new TesteObj3();
			obj.TesteObj3.ID = 5;
	
			Assert.AreEqual(true, obj.insert(), "DataObject cannot insert into table");
			Assert.AreEqual("INSERT INTO TesteObj (Cod,Nome,Idade,PesoKg,Teste1,Ativo) Values (1,'teste',18,60.5,5,'F')", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot insert into table");
		}
	
		[Test]
		public void FindFetch() {
			TesteObj obj = new TesteObj();
			obj.ID = 1;
			obj.Nome = "teste";
			obj.TesteObj3 = new TesteObj3();
			obj.TesteObj3.ID = 5;
			
			Assert.IsTrue(obj.find(), "DataObject find");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE (Cod = 1) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot select from table");
			Assert.IsTrue(obj.fetch(), "DataObject fetch from DataReader");

			obj = new TesteObj();
			obj.Nome = "teste";
			obj.Idade = 18;
			obj.Peso = 60.5;
			obj.TesteObj3 = new TesteObj3();
			obj.TesteObj3.ID = 5;			
			
			Assert.IsTrue(obj.find(), "DataObject find");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE (Nome LIKE 'teste') AND (Idade = 18) AND (PesoKg = 60.5) AND (Teste1 = 5) AND (Ativo = 'F') ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot select from table");
		}

		[Test]
		public void SetField() {
			TesteObj obj = new TesteObj();
			obj.ID = 2;
			obj.Nome = "teste 2";
			TesteObj3 obj3 = new TesteObj3();
			obj3.ID = 5;	
			obj.SetField("Teste1", obj3);
			
			Assert.AreEqual(obj3, obj.TesteObj3, "DataObject cannot set Field 'Nome'");
			obj.SetField("Teste1", 1);
			Assert.IsNull(obj.TesteObj3, "DataObject cannot set Field 'Teste1'");
			obj.SetField("Teste1", 5);
			Assert.AreEqual(5, obj.TesteObj3.ID, "DataObject cannot set Field 'Teste1'");
			obj3 = new TesteObj3();
			obj3.ID = 10;
			obj.SetField("TesteObj3", obj3);
			Assert.AreEqual(obj3, obj.TesteObj3, "DataObject cannot set Field 'TesteObj3'");
		}

		[Test, ExpectedException(typeof(CsDOException))]
		public void SetFieldError() {
			TesteObj4 obj = new TesteObj4();
			obj.ID = 1;
			obj.Nome = "teste";
			obj.SetField("TesteObj", 2);
		}
		
		[Test]
		public void Retrieve() {
            Conf.DataPool.Clear();
			TesteObj obj = new TesteObj();
			
			TesteObj3 obj3 = new TesteObj3();
			obj3.ID = 5;	
			
			Assert.IsTrue(obj.retrieve("Teste1", obj3), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE Teste1 = 5", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");
			Assert.IsTrue(obj.fetch(), "DataObject fetch");
			Assert.AreEqual(1, obj.ID, "DataObject retrieve");
			Assert.AreEqual(18, obj.Idade, "DataObject retrieve");
			Assert.AreEqual(60.5, obj.Peso, "DataObject retrieve");
			Assert.AreEqual(5, obj.TesteObj3.ID, "DataObject retrieve");
			Assert.AreEqual(50, obj.TesteObj3.Idade, "DataObject retrieve");
			Assert.AreEqual(100.5, obj.TesteObj3.Peso, "DataObject retrieve");
			Assert.AreEqual("teste5", obj.TesteObj3.Nome, "DataObject retrieve");
			Assert.AreEqual("teste1", obj.Nome, "DataObject retrieve");
			
			obj = new TesteObj();
			
			Assert.IsTrue(obj.retrieve("Teste1", 5), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE Teste1 = 5", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");
			Assert.IsTrue(obj.fetch(), "DataObject fetch");
			Assert.AreEqual(1, obj.ID, "DataObject retrieve");
			Assert.AreEqual(18, obj.Idade, "DataObject retrieve");
			Assert.AreEqual(60.5, obj.Peso, "DataObject retrieve");
			Assert.AreEqual(5, obj.TesteObj3.ID, "DataObject retrieve");
			Assert.AreEqual(50, obj.TesteObj3.Idade, "DataObject retrieve");
			Assert.AreEqual(100.5, obj.TesteObj3.Peso, "DataObject retrieve");
			Assert.AreEqual("teste5", obj.TesteObj3.Nome, "DataObject retrieve");
			Assert.AreEqual("teste1", obj.Nome, "DataObject retrieve");

			obj = new TesteObj();
			
			Assert.IsTrue(obj.retrieve("TesteObj3", 5), "DataObject retrieve");
			Assert.AreEqual("SELECT Cod,Nome,Idade,PesoKg,Aniversario,Teste1,Ativo FROM TesteObj WHERE Teste1 = 5", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject retrieve");
			Assert.IsTrue(obj.fetch(), "DataObject fetch");
			Assert.AreEqual(1, obj.ID, "DataObject retrieve");
			Assert.AreEqual(18, obj.Idade, "DataObject retrieve");
			Assert.AreEqual(60.5, obj.Peso, "DataObject retrieve");
			Assert.AreEqual(5, obj.TesteObj3.ID, "DataObject retrieve");
			Assert.AreEqual(50, obj.TesteObj3.Idade, "DataObject retrieve");
			Assert.AreEqual(100.5, obj.TesteObj3.Peso, "DataObject retrieve");
			Assert.AreEqual("teste5", obj.TesteObj3.Nome, "DataObject retrieve");
			Assert.AreEqual("teste1", obj.Nome, "DataObject retrieve");
		}
		
		[Test]
		public void Update() {
			TesteObj obj = new TesteObj();
			obj.ID = 1;
			obj.Nome = "teste2";
			obj.Idade = 22;
			obj.Peso = 70.5;
			obj.TesteObj3 = new TesteObj3();
			obj.TesteObj3.ID = 5;
			
			Assert.AreEqual(true, obj.update(), "DataObject cannot update table");
			Assert.AreEqual("UPDATE TesteObj SET Nome='teste2',Idade=22,PesoKg=70.5,Teste1=5,Ativo='F' WHERE (Cod = 1) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot update table");

			obj = new TesteObj();
			obj.ID = 2;
			obj.Nome = "teste3";
			obj.TesteObj3 = new TesteObj3();
			obj.TesteObj3.ID = 6;			
			
			Assert.AreEqual(true, obj.update(), "DataObject cannot update table");
			Assert.AreEqual("UPDATE TesteObj SET Nome='teste3',Teste1=6,Ativo='F' WHERE (Cod = 2) ", ((MockDriver)Conf.Driver).getPreviousCommand().CommandText, "DataObject cannot update table");		
		}
	}		
}
