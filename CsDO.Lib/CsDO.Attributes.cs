/*
 * Created by Alessandro Binhara
 * User: Administrator
 * Date: 29/4/2005
 * Time: 18:13
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
using System.Collections;
using System.Data;

namespace CsDO.Lib
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
	public class Table : Attribute
	{
		private string name = "";
		
		public Table(string name)
		{
			this.name = name;
		}

		public virtual string Name
		{
			get {return name;}
		}
	}


	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
	public class Column : Attribute
	{
		private string name = null;
		private bool persist = true;

		public Column(string name, bool persist)
		{
			this.name = name;
			this.persist = persist;
		}

		public Column(string name)
		{
			this.name = name;
		}

		public Column(bool persist)
		{
			this.persist = persist;
		}

		public virtual string Name
		{
			get {return name;}
		}

		public virtual bool Persist
		{
			get {return persist;}
			set {persist = value;}
		}
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
	public class PrimaryKey : Attribute
	{
		public PrimaryKey() {}
	}

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    public class Identity : Attribute
    {
        /*
         * This for a MSQL Server to get a id from database
         * Sample:
         * "INSERT INTO Departamento (nomeDep,emailDep,statusDep) 
         * Values ('Alessandro Binhara','binhara@yahoo.com','F') SELECT codigoDep=@@Identity FROM Departamento"
         * 
         * Table :
         * BEGIN
         * CREATE TABLE [dbo].[Departamento](
         * 	[codigoDep] [int] IDENTITY(1,1) NOT NULL,
         * 	[nomeDep] [varchar](30) NOT NULL,
         * 	[emailDep] [varchar](30) NOT NULL,
         * 	[statusDep] [varchar](1) NOT NULL DEFAULT ('A'),
         *  CONSTRAINT [Departamento_PK] PRIMARY KEY CLUSTERED 
         * (
         * 	[codigoDep] ASC
         * ) ON [PRIMARY]
         * ) ON [PRIMARY]
         * END
         * 
         * Store procedure
         * 

         * */
        public Identity() { }
    }
}
