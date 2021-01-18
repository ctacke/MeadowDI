using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DIMeadowApp
{
    public class ServiceCollection : IEnumerable<object>
    {
        private Dictionary<Type, object> m_items = new Dictionary<Type, object>();

        public IEnumerator<object> GetEnumerator()
        {
            return m_items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ServiceCollection()
        {
        }

        public void Add(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            lock (m_items)
            {
                var t = o.GetType();

                if (m_items.ContainsKey(t))
                {
                    throw new ArgumentException($"Object of type {t.Name} is already in the collection.");
                }
                m_items.Add(t, o);
            }
        }

        public void Add(object o, Type registerAs)
        {
            if (o == null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            lock (m_items)
            {
                if (m_items.ContainsKey(registerAs))
                {
                    throw new ArgumentException($"Object of type {registerAs.Name} is already in the collection.");
                }
                m_items.Add(registerAs, o);
            }
        }

        public void Add<TRegisterAs>(TRegisterAs o)
        {
            if (o == null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            lock (m_items)
            {
                var t = typeof(TRegisterAs);

                if (m_items.ContainsKey(t))
                {
                    throw new ArgumentException($"Object of type {t.Name} is already in the collection.");
                }
                m_items.Add(t, o);
            }
        }

        public object Get(Type registeredType)
        {
            lock (m_items)
            {
                if (m_items.ContainsKey(registeredType))
                {
                    return m_items[registeredType];
                }

                return m_items.Values.FirstOrDefault(t => t.GetType().IsAssignableFrom(registeredType));
            }
        }

        public TRegisteredType Get<TRegisteredType>()
        {
            var t = typeof(TRegisteredType);
            return (TRegisteredType)Get(t);
        }

        public TCreateType GetOrCreate<TCreateType>()
        {
            var existing = Get<TCreateType>();
            if (existing != null)
            {
                return existing;
            }
            return Create<TCreateType>();
        }

        public TRegisteredType GetOrCreate<TCreateType, TRegisteredType>()
            where TCreateType : TRegisteredType
        {
            var existing = Get<TRegisteredType>();
            if (existing != null)
            {
                return existing;
            }
            return Create<TCreateType, TRegisteredType>();
        }

        public TCreateType Create<TCreateType>()
        {
            var t = typeof(TCreateType);
            return (TCreateType)Create(t, null);
        }

        public TCreateType Create<TCreateType, TRegisterAs>()
            where TCreateType : TRegisterAs
        {
            var t1 = typeof(TCreateType);
            var t2 = typeof(TRegisterAs);
            return (TCreateType)Create(t1, t2);
        }

        public object Create(Type createType)
        {
            if (createType == null)
            {
                throw new ArgumentNullException(nameof(createType));
            }

            return Create(createType, null);
        }

        public object Create(Type createType, Type registerType)
        {
            if (createType == null)
            {
                throw new ArgumentNullException(nameof(createType));
            }

            if (registerType == null)
            {
                registerType = createType;
            }

            if (!registerType.IsAssignableFrom(createType))
            {
                throw new ArgumentException($"Type {createType.Name} does not inherit from {registerType.Name}");
            }

            // get the object's constructor(s)
            var ctors = createType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            // prefer a parameterless ctor
            var ctor = ctors.FirstOrDefault(c => c.GetParameters().Length == 0);
            object instance = null;
            if (ctor == null)
            {
                instance = DoContructorInjections(ctors);
            }
            else
            {
                instance = ctor.Invoke(null);
            }

            if (instance == null)
            {
                // TODO: look for a ctor we have enough info to inject into
                throw new Exception($"No parameterless constructor or injectable constructor found for {createType.Name}");
            }

            DoPropertyInjections(instance);

            lock (m_items)
            {
                if (m_items.ContainsKey(registerType))
                {
                    throw new ArgumentException($"Object of type {registerType.Name} is already in the collection.");
                }
                m_items.Add(registerType, instance);

                return instance;
            }
        }

        private object DoContructorInjections(IEnumerable<ConstructorInfo> ctors)
        {
            List<object> pList = new List<object>();

            foreach (var c in ctors)
            {
                pList.Clear();
                var valid = true;

                foreach (var p in c.GetParameters())
                {
                    var inject = m_items.Values.FirstOrDefault(i => p.ParameterType.IsAssignableFrom(i.GetType()));
                    if (inject == null)
                    {
                        valid = false;
                        break;
                    }
                    else
                    {
                        pList.Add(inject);
                    }
                }

                if (valid)
                {
                    return c.Invoke(pList.ToArray());
                }
            }
            return null;
        }

        private void DoPropertyInjections(object instance)
        {
            // Do basic property injection
            var settableProps = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(p => p.SetMethod != null);

            foreach (var p in settableProps)
            {
                var inject = m_items.Values.FirstOrDefault(i => p.PropertyType.IsAssignableFrom(i.GetType()));
                if (inject != null)
                {
                    // only inject if the property value is null (or has no getter)
                    if ((p.GetMethod == null) || (p.GetMethod.Invoke(instance, null) == null))
                    {
                        p.SetMethod.Invoke(instance, new object[] { inject });
                    }
                }
            }
        }
    }
}
