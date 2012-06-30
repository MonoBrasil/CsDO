/*
 * Created by SharpDevelop.
 * User: binhara
 * Date: 12/11/2005
 * Time: 15:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using NUnit.Framework;
using CsDO.Lib;

namespace CsDO.Tests 
{
	[TestFixture]
	public class TestConfigURL
	{
		[Test]
		public void TestUser()
		{
			Config.GetDbConectionString(Config.DBMS.PostgreSQL);
			Assert.AreEqual(Config.User, "postgres");
		}
		
		[Test]
		public void TestDataBase()
		{
			Config.GetDbConectionString(Config.DBMS.PostgreSQL);
			Assert.AreEqual(Config.Database, "csdo");
		}
		
		[Test]
		public void TestServer()
		{
			Config.GetDbConectionString(Config.DBMS.PostgreSQL);
			Assert.AreEqual(Config.Server, "localhost");
		}
		
		[Test]
		public void TestPort()
		{
			Config.GetDbConectionString(Config.DBMS.PostgreSQL);
			Assert.AreEqual(Config.Port, "5432");
		}
		
		[Test]
		public void TestPasswd()
		{
			Config.GetDbConectionString(Config.DBMS.PostgreSQL);
			Assert.AreEqual(Config.Password, "teste");
		}
		
		[Test]
		public void TestGetConnectionString()
		{
			string connStr = "Server=localhost;port=5432;User Id=postgres;Password=teste;Database=csdo;";
			Assert.AreEqual(connStr, Config.GetDbConectionString(Config.DBMS.PostgreSQL));
		}	
	
		[TestFixtureSetUp]
		public void Init()
		{
			// TODO: Add Init code.
		}
		
		[TestFixtureTearDown]
		public void Dispose()
		{
			// TODO: Add tear down code.
		}
	}
}
