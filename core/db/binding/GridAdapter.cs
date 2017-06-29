﻿using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xwcs.core.db.binding
{
    public interface IColumnAdapter
    {
        RepositoryItem ColumnEdit { get; set; }
        bool ReadOnly { get; set; }
        bool FixedWidth { get; set; }
        int Width { get; set; }

        AppearanceObjectEx AppearanceCell { get;}
    }

	/*
		Events merging
	*/
	public class CellValueChangedEventArgs : DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs
	{
		public bool GridLike { get; private set; }
		// grid
		public CellValueChangedEventArgs(DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs orig) : base(orig.RowHandle, orig.Column, orig.Value) {
			GridLike = true;
		}

		// tree
		public DevExpress.XtraTreeList.Columns.TreeListColumn TreeColumn { get; }
		public DevExpress.XtraTreeList.Nodes.TreeListNode Node { get; }

		public CellValueChangedEventArgs(DevExpress.XtraTreeList.CellValueChangedEventArgs orig) : base(-1, null, orig.Value)
		{
			TreeColumn = orig.Column;
			Node = orig.Node;
			GridLike = false;
		}
	}
	public delegate void CellValueChangedEventHandler(object sender, xwcs.core.db.binding.CellValueChangedEventArgs e);



public interface IGridAdapter
    {        
        bool IsReady { get; }        
        bool AutoPopulateColumns { get; set; }
        RepositoryItemCollection RepositoryItems { get; }
        void ForceInitialize();
        object DataSource { get; set; }
        void PopulateColumns();
        IColumnAdapter ColumnByFieldName(string fn);
        int ColumnsCount();
        void ClearColumns();

		void PostChanges();

        event EventHandler DataSourceChanged;
        
		//For Grid
		event CustomRowCellEditEventHandler CustomRowCellEditForEditing;
        event EventHandler ShownEditor;
        event CustomColumnDisplayTextEventHandler CustomColumnDisplayText;
        event EventHandler ListSourceChanged;
		event BaseContainerValidateEditorEventHandler ValidatingEditor;
		event xwcs.core.db.binding.CellValueChangedEventHandler CellValueChanged;

		//For Tree		
		//event DevExpress.XtraTreeList.CellValueChangedEventHandler TreeCellChanged;
		//event DevExpress.XtraTreeList.CustomColumnDisplayTextEventHandler TreeCustomColumnDisplayText;

	}

    public class GridColumnAdapter : IColumnAdapter
    {
        private GridColumn _c;

        public GridColumnAdapter(GridColumn c)
        {
            _c = c;
        }

        public AppearanceObjectEx AppearanceCell
        {
            get
            {
                return _c.AppearanceCell;
            }
        }

        public RepositoryItem ColumnEdit
        {
            get
            {
                return _c.ColumnEdit;
            }

            set
            {
                _c.ColumnEdit = value;
            }
        }

        public bool FixedWidth
        {
            get
            {
                return _c.OptionsColumn.FixedWidth;
            }

            set
            {
                _c.OptionsColumn.FixedWidth = value;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return _c.OptionsColumn.ReadOnly;
            }

            set
            {
                _c.OptionsColumn.ReadOnly = value;
            }
        }

        public int Width
        {
            get
            {
                return _c.Width;
            }

            set
            {
                _c.Width = value;
            }
        }
    }

    public class TreeColumnAdapter : IColumnAdapter
    {
        private DevExpress.XtraTreeList.Columns.TreeListColumn _c;

        public TreeColumnAdapter(DevExpress.XtraTreeList.Columns.TreeListColumn c)
        {
            _c = c;
        }

        public AppearanceObjectEx AppearanceCell
        {
            get
            {
                return _c.AppearanceCell;
            }
        }

        public RepositoryItem ColumnEdit
        {
            get
            {
                return _c.ColumnEdit;
            }

            set
            {
                _c.ColumnEdit = value;
            }
        }

        public bool FixedWidth
        {
            get
            {
                return _c.OptionsColumn.FixedWidth;
            }

            set
            {
                _c.OptionsColumn.FixedWidth = value;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return _c.OptionsColumn.ReadOnly;
            }

            set
            {
                _c.OptionsColumn.ReadOnly = value;
            }
        }

        public int Width
        {
            get
            {
                return _c.Width;
            }

            set
            {
                _c.Width = value;
            }
        }
    }

    

    public class GridAdapter : IGridAdapter
    {
		private GridControl _grid;
        private GridView _view;

		

		public GridAdapter(GridControl g)
        {
            _grid = g;
            if (!(_grid.MainView is GridView))
                throw new ApplicationException("Main view of grid must be e GridView");
            _view = _grid.MainView as GridView;

			//forward events
			_view.CellValueChanged += _view_CellValueChanged;

        }

		private void _view_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
		{
			CellValueChanged.Invoke(sender, new CellValueChangedEventArgs(e));
		}

		public event EventHandler ListSourceChanged
        {
            add
            {
                _view.DataController.ListSourceChanged += value;
            }
            remove
            {
                _view.DataController.ListSourceChanged -= value;
            }
        }

        public RepositoryItemCollection RepositoryItems
        {
            get
            {
                return _grid.RepositoryItems;
            }
        }

        public object DataSource
        {
            get
            {
                return _grid.DataSource;
            }

            set
            {
                _grid.DataSource = value;
            }
        }

        public bool IsReady
        {
            get
            {
                return _view.DataController.IsReady;
            }
        }

        public bool AutoPopulateColumns
        {
            get
            {
                return _view.OptionsBehavior.AutoPopulateColumns;
            }

            set
            {
                _view.OptionsBehavior.AutoPopulateColumns = value;
            }
        }

		public event BaseContainerValidateEditorEventHandler ValidatingEditor
		{
            add
            {
                _view.ValidatingEditor += value;
            }
			remove
            {
                _view.ValidatingEditor -= value;
            }
        }

		public event xwcs.core.db.binding.CellValueChangedEventHandler CellValueChanged;

		public event CustomRowCellEditEventHandler CustomRowCellEditForEditing
        {
            add
            {
                _view.CustomRowCellEditForEditing += value;
            }
            remove
            {
                _view.CustomRowCellEditForEditing -= value;
            }
        }
        public event EventHandler DataSourceChanged
        {
            add
            {
                _grid.DataSourceChanged += value;
            }
            remove
            {
                _grid.DataSourceChanged -= value;
            }
        }

        public event EventHandler ShownEditor
        {
            add
            {
                _view.ShownEditor += value;
            }
            remove
            {
                _view.ShownEditor -= value;
            }
        }

        public event CustomColumnDisplayTextEventHandler CustomColumnDisplayText
        {
            add
            {
                _view.CustomColumnDisplayText += value;
            }
            remove
            {
                _view.CustomColumnDisplayText -= value;
            }
        }

        public void ForceInitialize()
        {
            _grid.ForceInitialize();
        }

        public void PopulateColumns()
        {
            _view.PopulateColumns();
        }

        public IColumnAdapter ColumnByFieldName(string fn)
        {
            return new GridColumnAdapter(_view.Columns.ColumnByFieldName(fn));
        }

        public int ColumnsCount()
        {
            return _view.Columns.Count;
        }

        public void ClearColumns()
        {
            _view.Columns.Clear();
        }
		
		public void PostChanges()
		{
			_view.PostEditor();
			_view.UpdateCurrentRow();
		}
		
	}

/******************************/
/*
/*		TreeListAdapter
/*
/******************************/
    

    public class TreeListAdapter : IGridAdapter
    {
		



		private DevExpress.XtraTreeList.TreeList _tree;

		public event BaseContainerValidateEditorEventHandler ValidatingEditor
		{
			add
			{
				_tree.ValidatingEditor += value;
			}
			remove
			{
				_tree.ValidatingEditor -= value;
			}
		}

		public event EventHandler ListSourceChanged
		{
			add
			{
				//TODO : missing DataController
				//_tree.DataController.ListSourceChanged += value;
			}
			remove
			{
				//_tree.DataController.ListSourceChanged -= value;
			}
		}


		public event xwcs.core.db.binding.CellValueChangedEventHandler CellValueChanged;

		public TreeListAdapter(DevExpress.XtraTreeList.TreeList tl)
        {
            _tree = tl;

			_tree.CellValueChanged += _tree_CellValueChanged;
        }

		private void _tree_CellValueChanged(object sender, DevExpress.XtraTreeList.CellValueChangedEventArgs e)
		{
			CellValueChanged.Invoke(sender, new CellValueChangedEventArgs(e));
		}

		public RepositoryItemCollection RepositoryItems
        {
            get
            {
                return _tree.RepositoryItems;
            }
        }

        public object DataSource
        {
            get
            {
                return _tree.DataSource;
            }

            set
            {
                _tree.DataSource = value;
            }
        }

        public bool IsReady
        {
            get
            {
                return true;
            }
        }

		public bool AutoPopulateColumns
		{
			get
			{
				return _tree.OptionsBehavior.AutoPopulateColumns;
			}

			set
			{
				_tree.OptionsBehavior.AutoPopulateColumns = value;
			}
		}

		public event CustomRowCellEditEventHandler CustomRowCellEditForEditing
        {
            add
            {
				//TODO : missing CustomRowCellEditForEditing
				//_tree.CustomRowCellEditForEditing += value;
			}
			remove
            {
                //_tree.CustomRowCellEditForEditing -= value;
            }
        }
        public event EventHandler DataSourceChanged
        {
            add
            {
                _tree.DataSourceChanged += value;
            }
            remove
            {
                _tree.DataSourceChanged -= value;
            }
        }

        public event EventHandler ShownEditor
        {
            add
            {
                _tree.ShownEditor += value;
            }
            remove
            {
                _tree.ShownEditor -= value;
            }
        }

        public event CustomColumnDisplayTextEventHandler CustomColumnDisplayText
        {
            add
            {			
				//_tree.CustomColumnDisplayText += value;
			}
			remove
            {
                //_tree.CustomColumnDisplayText -= value;
            }
        }

        public void ForceInitialize()
        {
            _tree.ForceInitialize();
        }

        public void PopulateColumns()
        {
            _tree.PopulateColumns();
        }

        public Component ColumnByFieldName(string fn)
        {
            return _tree.Columns.ColumnByFieldName(fn);
        }

        IColumnAdapter IGridAdapter.ColumnByFieldName(string fn)
        {
            return new TreeColumnAdapter(_tree.Columns.ColumnByFieldName(fn));
        }

        public int ColumnsCount()
        {
            return _tree.Columns.Count;
        }

        public void ClearColumns()
        {
            _tree.Columns.Clear();
        }

		public void PostChanges()
		{
			_tree.PostEditor();

			//TODO : UpdateCurrentRow
			_tree.Update();
		}
	}
}
