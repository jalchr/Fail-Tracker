﻿using System.Web.Mvc;
using FailTracker.Web.Infrastructure.ModelMetadata;
using FailTracker.Web.Infrastructure.ModelMetadata.Filters;
using StructureMap.Configuration.DSL;

namespace FailTracker.Web.Infrastructure
{
	public class ModelMetadataRegistry : Registry
	{
		public ModelMetadataRegistry()
		{
			For<ModelMetadataProvider>().Use<SolidModelMetadataProvider>();

			Scan(scan =>
			     	{
			     		scan.TheCallingAssembly();
			     		scan.AddAllTypesOf<IModelMetadataFilter>();
			     	});
		}
	}
}