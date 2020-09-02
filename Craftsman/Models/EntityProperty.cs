﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Craftsman.Models
{
    public class EntityProperty
    {
        private bool _isRequired = false;
        private bool _canManipulate = true;
        private bool _isForeignKey = false;

        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of property (e.g. string, int, DateTime?, etc.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Determines if the property will be filterable by the API
        /// </summary>
        public bool CanFilter { get; set; } = false;

        /// <summary>
        /// Determines if the property will be sortable by the API
        /// </summary>
        public bool CanSort { get; set; } = false;

        //TODO update to default to true unless primary key == true
        /// <summary>
        /// Determines if the property can be manipulated when creating or updating the associated entity
        /// </summary>
        public bool CanManipulate
        {
            get 
            {
                if (IsPrimaryKey)
                    return false;
                else
                    return true;
            }
            set => _canManipulate = value;
        }

        /// <summary>
        /// Designates the property as the primary key for the entity
        /// </summary>
        public bool IsPrimaryKey { get; set; } = false;

        /// <summary>
        /// Denotes a required field in the database
        /// </summary>
        public bool IsRequired
        {
            get
            {
                if (IsPrimaryKey || IsCompositeKey)
                    return true;
                else
                    return false;
            }
            set => _isRequired = value;
        }

        /// <summary>
        /// Designates the property as a foreign key for the entity
        /// </summary>
        public bool IsForeignKey 
        {
            get => ForeignKeyPropName != null;
            private set => _isForeignKey = value;
        }

        /// <summary>
        /// Captures the foreign key property name
        /// </summary>
        public string ForeignKeyPropName { get; set; }

        /// <summary>
        /// Designates the property as a composite key for the entity
        /// </summary>
        public bool IsCompositeKey { get; set; } = false;
    }
}
