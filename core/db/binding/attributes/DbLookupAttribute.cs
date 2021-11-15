﻿using System;
using DevExpress.XtraDataLayout;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using System.Linq;
using DevExpress.XtraEditors.Filtering;
using DevExpress.XtraGrid.Views.Base;
using System.Drawing;
using DevExpress.XtraEditors.Controls;

namespace xwcs.core.db.binding.attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DbLookupAttribute : CustomAttribute
	{
		public string DisplayMember { set; get; }
		public string ValueMember { set; get; }
        private bool _AcceptNewValue = false;
        public bool AcceptNewValue { get { return _AcceptNewValue; }  set { _AcceptNewValue = value; } }
		private int _popUpWidth = 0;
		private int _popUpHeight = 0;
        private bool _UseCtrlScroll = true;
        public bool UseCtrlScroll
        {
            get { return _UseCtrlScroll; }
            set { _UseCtrlScroll = value; }
        }
        public int PopUpWidth
		{
			get { return _popUpWidth; }
			set { _popUpWidth = value; }
		}

		public int PopUpHeight
		{
			get { return _popUpHeight; }
			set { _popUpHeight = value; }
		}


		public override bool Equals(object obj)
		{
			DbLookupAttribute o = obj as DbLookupAttribute;
			if (o != null)
			{
				return DisplayMember.Equals(o.DisplayMember) && ValueMember.Equals(o.ValueMember);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int multiplier = 23;
			if (hashCode == 0)
			{
				int code = 133;
				code = multiplier * code + DisplayMember.GetHashCode();
				code = multiplier * code + ValueMember.GetHashCode();
				hashCode = code;
			}
			return hashCode;
		}

		// data layout like container
		public override void applyRetrievingAttribute(IDataBindingSource src, FieldRetrievingEventArgs e)
		{
            e.EditorType = typeof(DevExpress.XtraEditors.GridLookUpEdit);// LookUpEdit);
		}
		public override void applyRetrievedAttribute(IDataBindingSource src, FieldRetrievedEventArgs e)
        {
            RepositoryItemGridLookUpEdit rle = e.RepositoryItem as RepositoryItemGridLookUpEdit;
            src.EditorsHost.onSetupLookUpGridEventData(this, new SetupLookUpGridEventData { FieldName = e.FieldName, DataBindingSource = src, Rle = rle });
            setupRle(src, rle, e.FieldName);
		}

		// grid like container
		public override void applyGridColumnPopulation(IDataBindingSource src, GridColumnPopulated e) {
			e.RepositoryItem = new RepositoryItemGridLookUpEdit();
		}
		public override void applyCustomRowCellEdit(IDataBindingSource src, CustomRowCellEditEventArgs e) {
            RepositoryItemGridLookUpEdit rle = e.RepositoryItem as RepositoryItemGridLookUpEdit;
			rle.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            src.EditorsHost.onSetupLookUpGridEventData(this, new SetupLookUpGridEventData { FieldName = e.Column.FieldName, DataBindingSource = src, Rle = rle });
        }
		public override void applyCustomEditShown(IDataBindingSource src, ViewEditorShownEventArgs e) {
            RepositoryItemGridLookUpEdit rle = e.RepositoryItem as RepositoryItemGridLookUpEdit;
            setupRle(src, rle, e.FieldName);
		}

		//filter control
		public override void applyCustomEditShownFilterControl(IDataBindingSource src, ShowValueEditorEventArgs e) {
            RepositoryItemGridLookUpEdit rle = new RepositoryItemGridLookUpEdit();
			e.CustomRepositoryItem = rle;
            setupRle(src, rle, e.CurrentNode.FirstOperand.PropertyName);
		}

        private void setupRle(IDataBindingSource src, RepositoryItemGridLookUpEdit rle, string fn) 
		{
			rle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
			rle.DisplayMember = DisplayMember;
			rle.ValueMember = ValueMember;
            rle.AcceptEditorTextAsNewValue = (AcceptNewValue ? DevExpress.Utils.DefaultBoolean.True: DevExpress.Utils.DefaultBoolean.False);
            rle.TextEditStyle = TextEditStyles.Standard;
            rle.PopupFormMinSize = new Size(_popUpWidth, _popUpHeight);
            rle.View.OptionsView.ShowFilterPanelMode = ShowFilterPanelMode.Default;
            rle.View.OptionsView.ShowAutoFilterRow = true;
            rle.UseCtrlScroll = _UseCtrlScroll;
            GetFieldOptionsListEventData qd = new GetFieldOptionsListEventData { Data = null, FieldName = fn, DataBindingSource = src };
            src.EditorsHost.onGetOptionsList(this, qd);
			if (qd.Data != null)
			{
				rle.DataSource = qd.Data;
			}

        }

        public override void applyGetFieldDisplayText(IDataBindingSource src, CustomColumnDisplayTextEventArgs e) {
            src.EditorsHost.onGetFieldDisplayText(src, e);
        }
    }
}