﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace xwcs.core.db.fo
{
    using DevExpress.Data.Filtering;
    using model;
    using model.attributes;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Runtime.Serialization;
    using System.Reflection;
    using evt;
    using System.Diagnostics;
    using System.Reflection.Emit;
    using System.Linq.Expressions;
    using DevExpress.Data.Linq;
    using binding.attributes;




    /*
	 * NOTE:
	 *	Filter object list must be BindingList => so Binding Source can handle change of values
	 */
    [CollectionDataContract(IsReference = true)]
	public class FilterObjectList<T> : BindingList<T> where T : ICriteriaTreeNode {}

	[DataContract(IsReference = true)]
	public class FilterObjectsCollection <T> : INotifyPropertyChanged, INotifyModelPropertyChanged, ICriteriaTreeNode where T : ICriteriaTreeNode
	{
        private WeakEventSource<PropertyChangedEventArgs> _wes_PropertyChanged = null;
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_wes_PropertyChanged == null)
                {
                    _wes_PropertyChanged = new WeakEventSource<PropertyChangedEventArgs>();
                }
                _wes_PropertyChanged.Subscribe(value);
            }
            remove
            {
                _wes_PropertyChanged?.Unsubscribe(value);
            }
        }

        private WeakEventSource<ModelPropertyChangedEventArgs> _wes_ModelPropertyChanged = null;
        public event EventHandler<ModelPropertyChangedEventArgs> ModelPropertyChanged
        {
            add
            {
                if (_wes_ModelPropertyChanged == null)
                {
                    _wes_ModelPropertyChanged = new WeakEventSource<ModelPropertyChangedEventArgs>();
                }
                _wes_ModelPropertyChanged.Subscribe(value);
            }
            remove
            {
                _wes_ModelPropertyChanged?.Unsubscribe(value);
            }
        }

        #region serialize
        [DataMember(Order = 0)]
		private FilterObjectList<T> _data;
		[DataMember(Order = 1)]
		private string _fieldName;
		[DataMember(Order = 2)]
		private string _fieldFullName;

        

        // de serialization
        protected virtual void wakeup() {; }
        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            _data.ListChanged += internal_data_ListChanged;
            wakeup();
            _changed = false;
        }

        #endregion

        #region ICriteriaTreeNode
        public CriteriaOperator GetCondition()
		{
            //return _data.Count > 0 ? new ContainsOperator(GetFullFieldName(), CriteriaOperator.And(_data.Select(o => o.GetCondition()).AsEnumerable())) : null;
            return _data.Count > 0 ? CriteriaOperator.And(_data.Select(o => new ContainsOperator(GetFullFieldName(), o.GetCondition())).AsEnumerable()) : null;
                
        }
		public string GetFullFieldName()
		{
			return _fieldFullName;
		}
		public string GetFieldName()
		{
			return _fieldName;
		}
		public bool HasCriteria()
		{
			return true;
		}
		public void Reset()
		{
			Data = null;
		}
        
        #endregion


        //private need for de-serialize
        private FilterObjectsCollection() : this("", "") {}
		public FilterObjectsCollection(string pn, string fn) : base()
		{
			_data = new FilterObjectList<T>();
            _data.ListChanged += internal_data_ListChanged;
			_fieldName = fn;
			_fieldFullName = pn != "" ? pn + "." + fn : fn;
		}

        private void internal_data_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(e.ListChangedType == ListChangedType.ItemChanged)
            {

                // handle changed
                _changed = true;

                if(e.PropertyDescriptor != null)
                {
                    //froward event 
                    _wes_ModelPropertyChanged?.Raise(
                            this,
                            new ModelPropertyChangedEventArgs(
                                e.PropertyDescriptor.Name,
                                new ModelPropertyChangedEventArgs.PropertyChangedChainEntry()
                                {
                                    Container = _data[e.NewIndex],
                                    PropertyDescriptor = e.PropertyDescriptor,
                                    PropertyName = e.PropertyDescriptor.Name,
                                    Position = e.NewIndex,
                                    Collection = this
                                }
                            )
                   );
                }
                else
                {
                _wes_ModelPropertyChanged?.Raise(
                        this,
                        new ModelPropertyChangedEventArgs(
                            "*",
                            new ModelPropertyChangedEventArgs.PropertyChangedChainEntry()
                            {
                                Container = _data[e.NewIndex],
                                Position = e.NewIndex,
                                Collection = this
                             }
                        )
                   );
                }               
            }
        }

        public ICollection<T> Data {
			get {
				return _data;
			}
			set {
                if(ReferenceEquals(_data, value))
                {
                    return;
                }
				_data.Clear();
				if(value != null)
					value.ToList().ForEach(e => _data.Add(e));
			}
		}

        private bool _changed = false;
        public bool IsChanged()
        {
           return _changed;
        }
    } 


	[TypeDescriptionProvider(typeof(HyperTypeDescriptionProvider))]
	[DataContract(IsReference = true)]
	public abstract class FilterObjectbase : INotifyPropertyChanged, INotifyModelPropertyChanged, ICriteriaTreeNode, IModelEntity
	{
        protected static manager.ILogger _logger = manager.SLogManager.getInstance().getClassLogger(typeof(FilterObjectbase));
        private TypeCacheData _tcd;

        // this is for some FO internals handling
        internal class fake_filter_field
        {
            public string name;
            public Type type;
        }

        
        private WeakEventSource<PropertyChangedEventArgs> _wes_PropertyChanged = null;
		public event PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				if (_wes_PropertyChanged == null)
				{
					_wes_PropertyChanged = new WeakEventSource<PropertyChangedEventArgs>();
				}
                _wes_PropertyChanged.Subscribe(value);
            }
			remove {
                _wes_PropertyChanged?.Unsubscribe(value);
            }
		}

        private WeakEventSource<ModelPropertyChangedEventArgs> _wes_ModelPropertyChanged = null;
        public event EventHandler<ModelPropertyChangedEventArgs> ModelPropertyChanged
        {
            add
            {
                if (_wes_ModelPropertyChanged == null)
                {
                    _wes_ModelPropertyChanged = new WeakEventSource<ModelPropertyChangedEventArgs>();
                }
                _wes_ModelPropertyChanged.Subscribe(value);
            }
            remove
            {
                _wes_ModelPropertyChanged?.Unsubscribe(value);
            }
        }

        private bool _changed = false;
        public bool IsChanged()
        {
            return _changed;
        }

        private CriteriaToEFExpressionConverter _converter = null;
        //private CriteriaToExpressionConverter _converter = null;
        

        #region serialize
        [DataMember(Order = 0)]
		private string _fieldName;
		[DataMember(Order = 1)]
		private string _fieldFullName;
        [DataMember(Order = 2)]
        private bool _isAdvanced;
        [DataMember(Order = 3)]
        private string _advancedCriteriaString
        {
            get
            {
                return !ReferenceEquals(null, _advancedCriteria) ? _advancedCriteria.ToString() : "";
            }
            set
            {
                _advancedCriteria = value == "" ? null : CriteriaOperator.Parse(value);
            }
        }

        // de serialization
        protected virtual void wakeup() {}
        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            BindToNesteds();
            wakeup();

            //restore default state
            _changed = false;
        }
        #endregion

        // advanced criteria
        private CriteriaOperator _advancedCriteria;

        // hold cached fake filter object
        private object _fakeFilterObject;

		#region ICriteriaTreeNode
		public CriteriaOperator GetCondition() {
            return _isAdvanced ? _advancedCriteria : GetCriteriaOperator();
		}
		public string GetFullFieldName()
		{
			return _fieldFullName;
		}
		public string GetFieldName()
		{
			return _fieldName;
		}
		public void Reset()
		{
            
            GetType()
			.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
			.Select(field => field.GetValue(this))
            .Where(val => (val is ICriteriaTreeNode))
			.Cast<ICriteriaTreeNode>()
            //.Where(c => !ReferenceEquals(c, null))
			.ToList()
			.ForEach(c => { c.Reset(); } );

            GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(field => field.GetValue(this))
            .Where(val => (val is String))
            .Cast<String>()
            //.Where(c => !ReferenceEquals(c, null))
            .ToList()
            .ForEach(c => { c = null; });

            GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(field => field.GetValue(this))
            .Where(val => (val is DateTime?))
            .Cast<DateTime?>()
            //.Where(c => !ReferenceEquals(c, null))
            .ToList()
            .ForEach(c => { c = null; });

            //invoke property changed on last property, it will force update all bindings anyway
            OnPropertyChanged(String.Empty);

            // advanced
            _isAdvanced = false;
            _advancedCriteria = null;
        }
        #endregion

        /*
         * Lets say we need access all property getters with array[name] notation
         */
        protected static void InitReflectionChache(Type who)
        {
            TypeCache.GetTypeCacheData(who);
        }

        public FilterObjectbase() : this("", "") {}
        public FilterObjectbase(string pn, string fn)
        {
            if (_wes_PropertyChanged == null) _wes_PropertyChanged = new WeakEventSource<PropertyChangedEventArgs>();
            _fieldName = fn;
            _fieldFullName = pn != "" ? pn + "." + fn : fn;
            _isAdvanced = false;
            _advancedCriteria = null;
            _fakeFilterObject = null;


            //call eventual init in partial
            MethodInfo mi = GetType().GetMethod("InitPartial");
            if(mi != null)
            {
                mi.Invoke(this, null);
            }
        }
        
        protected virtual CriteriaOperator GetMixedCriteria()
        {
            return _isAdvanced ? _advancedCriteria : GetCriteriaOperator();
        }     

        public Expression GetLinqExpression<T>()
        {
            if(ReferenceEquals(null, _converter))
            {
                _converter = new CriteriaToEFExpressionConverter();
                //_converter = new CriteriaToExpressionConverter();


            }
            return  _converter.Convert(Expression.Parameter(typeof(T), "el"), GetMixedCriteria());
        }

        // return fake filter object
        public object GetFakeFilterObject()
        {
            if (_fakeFilterObject == null)
            {
                AssemblyBuilder dynamicAssembly =
                    AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("xwcs.dynamic.data.assembly"),
                        AssemblyBuilderAccess.Run);
                ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("xwcs.dynamic.data.assembly.Module");
                _fakeFilterObject = Activator.CreateInstance(CreateFakeFilterObjectType(GetType().Name, GetType(), dynamicModule));
            }

            return _fakeFilterObject;
        }

        

        // take current condition from fields and save it in criteria string
        public void SaveToAdvancedCriteria()
        {
            _advancedCriteria = GetCriteriaOperator();
            _changed = true;
        }


        // cant use getter/setter
        public CriteriaOperator GetAdvancedCriteria()
        {
            if (ReferenceEquals(_advancedCriteria, null))
            {
                SaveToAdvancedCriteria();
            }
            return _advancedCriteria;
        }

        public void  SetAdvancedCriteria(CriteriaOperator c)
        {
            if (ReferenceEquals(null, _advancedCriteria) || !_advancedCriteria.Equals(c))
            {
                _advancedCriteria = c;
                _changed = true;
            }            
        }
        public bool GetIsAdvanced()  
        {
            return _isAdvanced;

        }
        public void SetIsAdvanced(bool a)
        {
            _isAdvanced = a;
            _changed = true;
        }

        public ICriteriaTreeNode GetFilterFieldByPath(string path)
        {
            return GetFilterFieldByPath(this, path);
        }

        public bool HasCriteria()
        {
            return true;
        }

        public void ResetFieldByName(string FieldName)
        {
            GetFilterFieldByPath(FieldName)?.Reset();
            OnPropertyChanged(FieldName, null, ModelPropertyChangedEventKind.Reset);
        }

        protected void SetField<T>(ref FilterField<T> storage, object value, [CallerMemberName] string propertyName = null)
		{
            // skip if not changed
            if (storage.ValueEquals(value)) return;

#if DEBUG_TRACE_LOG_ON
            MethodBase info = new StackFrame(3).GetMethod();
            _logger.Debug(string.Format("{0}.{1} -> {2}", info.DeclaringType.Name, info.Name, propertyName));
#endif

            storage.Value = (T)value;
            OnPropertyChanged(propertyName, storage);
        }

        private void OnNestedPropertyChanged(object sender, ModelPropertyChangedEventArgs e)
        {
            // add root type name to nested property changed event
            if (e.AddInChain((sender as ICriteriaTreeNode)?.GetFieldName(),
                            new ModelPropertyChangedEventArgs.PropertyChangedChainEntry()
                            {
                                PropertyName = (sender as ICriteriaTreeNode)?.GetFieldName(),
                                Container = this
                            }
            ))
            {
                // handle changed
                _changed = true; // false positive it will do true even if object was empty

                // notify
                _wes_ModelPropertyChanged?.Raise(this, e);
            }            
        }

        protected void SetField<T>(ref T storage, object value, [CallerMemberName] string propertyName = null)
        {
            // skip if not changed
            if (ReferenceEquals(storage, value)) return;
#if DEBUG_TRACE_LOG_ON
                MethodBase info = new StackFrame(3).GetMethod();
            _logger.Debug(string.Format("{0}.{1} -> {2}", info.DeclaringType.Name, info.Name, propertyName));
#endif
            // handle real change
            if (!ReferenceEquals(null, storage))
            {
                //remove old changed handler
                (storage as INotifyModelPropertyChanged).ModelPropertyChanged -= OnNestedPropertyChanged;
            }

            storage = (T)value;
            // new handler
            (storage as INotifyModelPropertyChanged).ModelPropertyChanged += OnNestedPropertyChanged;
            // notify that nested was changed
            OnPropertyChanged(propertyName, storage);
        }

        protected void SetField<T>(ref FilterObjectsCollection<T> storage, ICollection<T> value, [CallerMemberName] string propertyName = null) where T : ICriteriaTreeNode
        {
            // skip if not changed
            if (ReferenceEquals(storage.Data, value)) return;
#if DEBUG_TRACE_LOG_ON
            MethodBase info = new StackFrame(3).GetMethod();
            _logger.Debug(string.Format("{0}.{1} -> {2}", info.DeclaringType.Name, info.Name, propertyName));
#endif
            // handle real change
            if (!ReferenceEquals(null, storage))
            {
                //remove old changed handler
                (storage as INotifyModelPropertyChanged).ModelPropertyChanged -= OnNestedPropertyChanged;
            }

            storage.Data = value;
            // new handler
            (storage as INotifyModelPropertyChanged).ModelPropertyChanged += OnNestedPropertyChanged;
            // notify that nested was changed
            OnPropertyChanged(propertyName, storage);
        }



        protected void SetFieldCriteria<T>(ref FilterField<T> storage, CriteriaOperator value, [CallerMemberName] string propertyName = null)
		{
#if DEBUG_TRACE_LOG_ON
            MethodBase info = new StackFrame(3).GetMethod();
            _logger.Debug(string.Format("{0}.{1} -> {2}", info.DeclaringType.Name, info.Name, propertyName));
#endif
            var oldValue = storage.Value;	
			storage.Condition = value;
			//condition set value to null, so if old was something else notify property change
			if(oldValue != null)
                OnPropertyChanged(propertyName, storage);
        }

        //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        protected void OnPropertyChanged(string propertyName = null, object value = null, ModelPropertyChangedEventKind kind = ModelPropertyChangedEventKind.SetValue)
        {

            // handle changed
            _changed = true; // false positive it will do true even if object was empty

#if DEBUG_TRACE_LOG_ON
            _logger.Debug(string.Format("Property changed {0} from  {1}", propertyName, GetType().Name));
#endif
            _wes_PropertyChanged?.Raise(this, new PropertyChangedEventArgs(propertyName));

            // model changes
            _wes_ModelPropertyChanged?.Raise(
                this,
                new ModelPropertyChangedEventArgs(
                    propertyName == string.Empty ? "*" : propertyName,
                    new ModelPropertyChangedEventArgs.PropertyChangedChainEntry()
                    {
                        Container = this,
                        PropertyName = propertyName == string.Empty ? "*" : propertyName,
                        Value = value
                    },
                    kind
                )
            );
        }

        private ICriteriaTreeNode GetFilterFieldByPath(ICriteriaTreeNode obj, string path) {
			if (obj == null) { return null; }

			int l = path.IndexOf(".");
			string name = path;
			string suffix = "";
			if (l > 0)
			{
				name = path.Substring(0, l);
				suffix = path.Substring(l + 1);
			}

            try {
				obj = obj.GetType()
				.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(field => field.Name == '_' + name.LowercaseFirst())
				.Select(field => field.GetValue(obj))
				.Single() as ICriteriaTreeNode;
			}catch(Exception) {
				return null;
			}			
			
			//not done			
			if(suffix != "") {
				return GetFilterFieldByPath(obj, suffix);
			}

			//done
			return obj;
		}

        protected virtual CriteriaOperator GetCriteriaOperator()
        {
            //build condition
            return CriteriaOperator.And(
                    this.GetType()
                    .GetFields(System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(field => field.GetValue(this))
                    .Cast<ICriteriaTreeNode>()
                    .Select(c => c.GetCondition())
                    .AsEnumerable()
            );
        }


        // make filter fake object
        private Type CreateFakeFilterObjectType(string name, Type t, ModuleBuilder mb = null)
        {
            // make first type from all fields
            TypeBuilder ntb = mb.DefineType(name + "_t", TypeAttributes.Public);

            // fields   
            IEnumerable<FieldInfo> fields = t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            fields.Where(f => f.FieldType.IsSubclassOfRawGeneric(typeof(FilterField<>)))
            .Select(f => {
                // we need handle correctly name of field cause private filed will lowercased first letter just
                // after first _
                // so we have to attempt to find property which need to match
                String tmpName = f.Name.Substring(f.Name.IndexOf('_') + 1);
                PropertyInfo pi = t.GetProperty(tmpName, BindingFlags.Public | BindingFlags.Instance);
                if(ReferenceEquals(null, pi))
                {
                    // try with uppercased first letter
                    pi = t.GetProperty(tmpName.UppercaseFirst(), BindingFlags.Public | BindingFlags.Instance);
                }
                tmpName = ReferenceEquals(null, pi) ? tmpName : pi.Name;
                return new fake_filter_field()
                {
                    name = tmpName,  //  f.Name.Substring(f.Name.IndexOf('_') + 1),
                    // filter field is template so take its first generic argument and check it if it is generic again
                    type = f.FieldType.GenericTypeArguments[0].IsGenericType ?
                            // nullable<>
                            f.FieldType.GenericTypeArguments[0].GenericTypeArguments[0] :
                            // direct type
                            f.FieldType.GenericTypeArguments[0]
                };
            }).ToList()
            .ForEach(c => { ReflectionHelper.AddProperty(ntb, c.name, c.type); });

            // nested collections
            fields.Where(f => f.FieldType.IsSubclassOfRawGeneric(typeof(FilterObjectsCollection<>)))
            .Select(f => {
                // we need handle correctly name of field cause private filed will lowercased first letter just
                // after first _
                // so we have to attempt to find property which need to match
                String tmpName = f.Name.Substring(f.Name.IndexOf('_') + 1);
                PropertyInfo pi = t.GetProperty(tmpName, BindingFlags.Public | BindingFlags.Instance);
                if (ReferenceEquals(null, pi))
                {
                    // try with uppercased first letter
                    pi = t.GetProperty(tmpName.UppercaseFirst(), BindingFlags.Public | BindingFlags.Instance);
                }
                tmpName = ReferenceEquals(null, pi) ? tmpName : pi.Name;
                return new fake_filter_field()
                {
                    name = tmpName,
                    type = f.FieldType.GenericTypeArguments[0]
                };
            }).ToList()
            .ForEach(c =>
            {
                ReflectionHelper.AddProperty(
                    ntb,
                    c.name,
                    typeof(List<>).MakeGenericType(new Type[] { CreateFakeFilterObjectType(c.name, c.type, mb) })
                );
            });

            // nested objects
            fields.Where(f => f.FieldType.IsSubclassOfRawGeneric(typeof(FilterObjectbase)))
            .Select(f => {
                // we need handle correctly name of field cause private filed will lowercased first letter just
                // after first _
                // so we have to attempt to find property which need to match
                String tmpName = f.Name.Substring(f.Name.IndexOf('_') + 1);
                PropertyInfo pi = t.GetProperty(tmpName, BindingFlags.Public | BindingFlags.Instance);
                if (ReferenceEquals(null, pi))
                {
                    // try with uppercased first letter
                    pi = t.GetProperty(tmpName.UppercaseFirst(), BindingFlags.Public | BindingFlags.Instance);
                }
                tmpName = ReferenceEquals(null, pi) ? tmpName : pi.Name;
                return new fake_filter_field()
                {
                    name = tmpName,
                    type = f.FieldType
                };
            }).ToList()
            .ForEach(c =>
            {
                ReflectionHelper.AddProperty(
                    ntb,
                    c.name,
                    CreateFakeFilterObjectType(c.name, c.type, mb)
                );
            });

            // now make property
            return ntb.CreateType();
        }

        protected virtual void AfterBindToNesteds()
        {
            // can be overriden
        }

        protected void BindToNesteds()
        {
            Type selfT = GetType();

            // fields   
            selfT.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.FieldType.IsSubclassOfRawGeneric(typeof(FilterObjectbase)) || f.FieldType.IsSubclassOfRawGeneric(typeof(FilterObjectsCollection<>)))
            .Select(field => field.GetValue(this))
            .Cast<INotifyModelPropertyChanged>()
            .ToList()
            .ForEach(c =>
            {
                if (!ReferenceEquals(null, c))
                {
                    //remove old changed handler
                    c.ModelPropertyChanged += OnNestedPropertyChanged;
                }
            });

            AfterBindToNesteds();


            if (ReferenceEquals(null, _tcd))
            {
                _tcd = TypeCache.GetTypeCacheData(GetType());
            }
            // handle conversion attributes for fields
            IEnumerable<PropertyInfo> pps = _tcd.GetPropertiesWithAttributeType(typeof(CustomConverterAttribute)).ToList();

            foreach (PropertyInfo pi in pps)
            {

                IHasConverter ff = selfT.GetField("_" + pi.Name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this) as IHasConverter;

                if(!ReferenceEquals(null, ff))
                {
                    CustomConverterAttribute a = _tcd.GetCustomTypedAttributesForProperty(pi.Name, typeof(CustomConverterAttribute)).First() as CustomConverterAttribute;
                    ff.Converter = a;
                }
            }
        }


        public object GetModelPropertyValueByName(string PropName)
        {
            return GetModelPropertyValueByNameInternal(this, PropName);
        }

        protected object GetModelPropertyValueByNameInternal(object obj, string path)
        {
            if (obj == null) { return null; }

            int l = path.IndexOf(".");
            string name = path;
            string suffix = "";
            if (l > 0)
            {
                name = path.Substring(0, l);
                suffix = path.Substring(l + 1);
            }

            try
            {
                obj = obj.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.Name == '_' + name)
                .Select(field => field.GetValue(obj))
                .Single();
            }
            catch (Exception)
            {
                return null;
            }

            //not done			
            if (suffix != "")
            {
                return GetModelPropertyValueByNameInternal(obj, suffix);
            }

            //done
            return obj;
        }

        public bool IsTheSame(FilterObjectbase right)
        {
            CriteriaOperator co = GetCondition();
            CriteriaOperator cor = right.GetCondition();

            return (ReferenceEquals(null, co) ? "" : co.ToString()).Equals(ReferenceEquals(null, cor) ? "" : cor.ToString());
        }

        public int GetDataVersion()
        {
            throw new NotImplementedException();
        }

        public void SetDataVersion(int value)
        {
            throw new NotImplementedException();
        }

        public DBContextBase GetCtx()
        {
            throw new NotImplementedException();
        }

        public void SetCtx(DBContextBase c)
        {
            throw new NotImplementedException();
        }
    }
}
