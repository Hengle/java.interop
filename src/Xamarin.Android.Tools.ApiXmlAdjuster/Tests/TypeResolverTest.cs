﻿using System;
using System.Linq;
using NUnit.Framework;

namespace Xamarin.Android.Tools.ApiXmlAdjuster.Tests
{
	[TestFixture]
	public class TypeResolverTest
	{
		JavaApi api;
		
		[TestFixtureSetUp]
		public void SetupFixture ()
		{
			api = JavaApiTestHelper.GetLoadedApi ();
			api.Resolve ();
		}
		
		[Test]
		public void TypeReferenceEquals ()
		{
			var intRef = JavaTypeReference.Int;
			Assert.AreEqual (JavaTypeReference.Int, intRef, "primitive types 2");

			var dummyType = new JavaClass (new JavaPackage (api) { Name = string.Empty }) { Name = "Dummy" };
			var tps = new JavaTypeParameters (dummyType);
			var gt = new JavaTypeParameter (tps) { Name = "T" };
			Assert.AreEqual (new JavaTypeReference (gt, null), new JavaTypeReference (new JavaTypeParameter (tps) { Name = "T"}, null), "type parameters");
			Assert.AreNotEqual (new JavaTypeReference (gt, null), new JavaTypeReference (new JavaTypeParameter (tps) { Name = "U"}, null), "type parameters 2");
			Assert.AreNotEqual (new JavaTypeReference (gt, null), new JavaTypeReference (gt, "[]"), "type parameters: array vs. non-array");
			Assert.AreEqual (new JavaTypeReference (gt, "[]"), new JavaTypeReference (gt, "[]"), "type parameters: array vs. array");
			
			var type = new JavaClass (new JavaPackage (api) { Name = string.Empty }) { Name = "T" };
			Assert.AreEqual (new JavaTypeReference (type, null, null), new JavaTypeReference (type, null, null), "type vs. type");
			Assert.AreNotEqual (new JavaTypeReference (type, null, "[]"), new JavaTypeReference (type, null, null), "type: array vs. non array");
			Assert.AreNotEqual (new JavaTypeReference (type, null, "[]"), new JavaTypeReference (type, null, "[][]"), "type: array vs. array of array");
			Assert.AreNotEqual (new JavaTypeReference (type, null, null), new JavaTypeReference (new JavaTypeParameter (tps) { Name = "T"}, null), "type vs. type parameters");

			Assert.AreNotEqual (new JavaTypeReference (gt, "[]"), new JavaTypeReference (type, null, null), "type: array vs. non array");
			Assert.AreNotEqual (new JavaTypeReference (type, null, "[]"), new JavaTypeReference (type, null, "[][]"), "type: array vs. array of array");
		}
		
		[Test]
		public void TestResolvedTypes ()
		{
			var type = api.FindNonGenericType ("android.database.ContentObservable");
			Assert.IsNotNull (type, "type not found");
			var kls = type as JavaClass;
			Assert.IsNotNull (kls, "type was not class");
			Assert.IsNotNull (kls.ResolvedExtends, "extends not resolved.");
			Assert.IsNotNull (kls.ResolvedExtends.ReferencedType, "referenced type is not correctly resolved");
		}

		[Test]
		public void ResolveGenericArguments ()
		{
			var type = api.FindNonGenericType ("java.util.concurrent.ConcurrentHashMap");
			Assert.IsNotNull (type, "type not found");
			var kls = type as JavaClass;
			Assert.IsNotNull (kls, "type was not class");
			var method = kls.Members.OfType<JavaMethod> ().First (m => m.Name == "searchEntries");
			Assert.IsNotNull (method, "method not found.");
			var para = method.Parameters [1];
			Assert.AreEqual ("java.util.function.Function<java.util.Map.Entry<K, V>, ? extends U>",
			                 para.ResolvedType.ToString (),
			                 "referenced type is not correctly resolved");
		}
	}
}

