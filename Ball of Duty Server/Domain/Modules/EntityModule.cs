using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Entity.Components.Physics.Collision;
using Entity.Factories;

namespace Ball_of_Duty_Server.Domain.Modules
{
    public class EntityModule : Module
    {
        private const string ASSEMBLY_NAME = "EntityImpl";
        private const string FULLY_QUALIFIED_TYPE_NAME = ASSEMBLY_NAME + ".Factories.EntityFactory";
        private static string _dllPath = Path.Combine(_appDomainPath, $@"{ASSEMBLY_NAME}.dll");
        private TempClass _tempClass;

        public override object Factory => _tempClass;

        public EntityModule()
        {
            Reload();
        }

        public override sealed void Reload()
        {
            _tempClass = new TempClass();
            Assembly asm = base.ReloadAssembly(ASSEMBLY_NAME, _dllPath).Assembly;
            _tempClass.EntityFactory = (IEntityFactory)asm.CreateInstance(FULLY_QUALIFIED_TYPE_NAME);
            _tempClass.CollisionHandler = (ICollisionHandler)asm.CreateInstance("EntityImpl.Components.Physics.Collision.CollisionHandler");
        }

        public class TempClass
        {
            public IEntityFactory EntityFactory { get; set; }
            public ICollisionHandler CollisionHandler { get; set; }
        }
    }
}