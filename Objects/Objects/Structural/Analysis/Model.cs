﻿using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;


namespace Objects.Structural.Analysis
{
    public class Model : Base
    {        
        public ModelInfo specs { get; set; } //container for model and project specifications

        [DetachProperty]
        [Chunkable(5000)]
        public List<Base> nodes { get; set; } //nodes list

        [DetachProperty]
        [Chunkable(5000)]
        public List<Base> elements { get; set; } //element (or member) list

        [DetachProperty]
        [Chunkable(5000)]
        public List<Base> loads { get; set; } //loads list

        [DetachProperty]
        [Chunkable(5000)]
        public List<Base> restraints { get; set; } //supports list

        [DetachProperty]
        [Chunkable(5000)]
        public List<Base> properties { get; set; } //properties list

        [DetachProperty]
        [Chunkable(5000)]
        public List<Base> materials { get; set; } //materials list

        public string layerDescription { get; set; } //design layer, analysis layer
        public Model() { }

        /// <summary>
        /// SchemaBuilder constructor for a structural model object
        /// </summary>
        /// <param name="modelInfo"></param>
        /// <param name="nodes"></param>
        /// <param name="elements"></param>
        /// <param name="loads"></param>
        /// <param name="restraints"></param>
        /// <param name="properties"></param>
        /// <param name="materials"></param>
        [SchemaInfo("Model", "Creates a Speckle structural model object", "Structural", "Analysis")]
        public Model(ModelInfo specs, List<Base> nodes, List<Base> elements, List<Base> loads, List<Base> restraints, List<Base> properties, List<Base> materials) 
        {
            this.specs = specs;
            this.nodes = nodes;
            this.elements = elements;
            this.loads = loads;
            this.restraints = restraints;
            this.properties = properties;
            this.materials = materials;
        }
    }
}
