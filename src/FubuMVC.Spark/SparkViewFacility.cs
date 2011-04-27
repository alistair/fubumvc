﻿using System;
using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.View;
using FubuMVC.Spark.SparkModel;

namespace FubuMVC.Spark
{
    public class SparkViewFacility : IViewFacility
    {
        private readonly IEnumerable<SparkItem> _sparkItems;
        public SparkViewFacility(IEnumerable<SparkItem> sparkItems)
        {
            _sparkItems = sparkItems;
        }

        public IEnumerable<IViewToken> FindViews(TypePool types, BehaviorGraph graph)
        {
            return _sparkItems
                .Where(x => x.HasViewModel())
                .Select(item => new SparkViewToken(item));
        }

        public BehaviorNode CreateViewNode(Type type)
        {
            return null;
        }
    }
}