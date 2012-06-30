/*
 * Created by Alexandre Rocha Lima e Marcondes
 * User: Administrator
 * Date: 12/08/2005
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
using System.Collections;
using System.Runtime.Remoting;

namespace CsDO.Lib
{
    [Serializable]
	public class Singleton
	{
		private static readonly Hashtable instances = new Hashtable();
        private static object syncRoot = new object();
		private static volatile int numOfReferences = 0;
		private static bool innerCall = false;

		public Singleton() { 
			if (!innerCall)
				throw new TypeInitializationException("This class is Singleton, use New() instead of the class constructor.", new Exception(""));
		}

		protected static object Instance(Type self)
		{
			Singleton instance = null;
			
			innerCall = true;

			if (self.IsSubclassOf(typeof(Singleton)) || self == typeof(Singleton))
			{          		
				instance = (Singleton) instances[self];
				if (instance == null)
	            {
					lock(syncRoot)
	                {
						instance = (Singleton) instances[self];
						if (instance == null) {
							instance = (Singleton) Activator.CreateInstance(self);
							instances.Add(self, instance);
						}
	            	}
	        	}

				if (instance == null)
        			throw new TypeInitializationException("Default constructor not called.", new Exception(""));	
			} 
			
			innerCall = false;

			numOfReferences++;
			if ((numOfReferences == 1) && (instance != null))
				instance.Init();

			return instance;
		}
    
		public static int References
		{
			get { return numOfReferences; }
		} 

		virtual protected void Init() { }
		public static Singleton New() { return (Singleton) Instance(typeof(Singleton)); }
	}
}

