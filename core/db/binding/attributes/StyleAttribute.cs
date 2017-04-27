﻿/*
using System;
using DevExpress.XtraDataLayout;

namespace xwcs.core.db.binding.attributes
{
	[AttributeUsage(AttributeTargets.Property,	AllowMultiple = true)]
	public class StyleAttribute : CustomAttribute
	{
		uint _backGrndColor;
		bool _backGrndColorUsed;

		public override bool Equals(object obj)
		{
			StyleAttribute o = obj as StyleAttribute;
			if(o != null) {
				return _backGrndColor == o._backGrndColor && _backGrndColorUsed == o._backGrndColorUsed;
			}
            return false;
		}

		public override int GetHashCode()
		{
			int multiplier = 23;
			if (hashCode == 0)
			{
				int code = 133;
				code = multiplier * code + (int)_backGrndColor;
				code = multiplier * code + (_backGrndColorUsed ? 1 : 0);
				hashCode = code;
			}
			return hashCode;
		}
		public StyleAttribute() {
			_backGrndColorUsed = false; //default
		}

		public uint BackgrounColor
		{
			get { return _backGrndColor; }
			set { _backGrndColorUsed = true;  _backGrndColor = value; }
		}

		public override void applyRetrievedAttribute(IDataBindingSource src, FieldRetrievedEventArgs e) {
			if (_backGrndColorUsed)
			{
				e.Control.BackColor = System.Drawing.Color.FromArgb((int)_backGrndColor);
			}
		}
	}	
}
*/

using System;
using DevExpress.XtraDataLayout;
using DevExpress.XtraEditors;

namespace xwcs.core.db.binding.attributes
{
	[AttributeUsage(AttributeTargets.Property,	AllowMultiple = true)]
	public class StyleAttribute : CustomAttribute
	{
		StyleController _styleController = new StyleController();
		uint _backGrndColor;
		uint _backGrndColorDisabled;
		uint _backGrndColorFocused;
		uint _backGrndColorReadOnly;


        // separate styling
        public int ColumnWidth { get; set; } = -1; // auto


		public StyleAttribute()
		{
		}

		public uint BackgrounColor
		{
			get { return _backGrndColor; }
			set 
			{ 
				_backGrndColor = value;
				_styleController.Appearance.BackColor = System.Drawing.Color.FromArgb((int)_backGrndColor);
			}
		}

		public uint BackGrndColorDisabled
		{
			get { return _backGrndColorDisabled; }
			set
			{
				_backGrndColorDisabled = value;
				_styleController.AppearanceDisabled.BackColor = System.Drawing.Color.FromArgb((int)_backGrndColorDisabled);
			}
		}

		public uint BackGrndColorFocused
		{
			get { return _backGrndColorFocused; }
			set
			{
				_backGrndColorFocused = value;
				_styleController.AppearanceFocused.BackColor = System.Drawing.Color.FromArgb((int)_backGrndColorFocused);
			}
		}

		public uint BackGrndColorReadOnly
		{
			get { return _backGrndColorReadOnly; }
			set
			{
				_backGrndColorReadOnly = value;
				_styleController.AppearanceReadOnly.BackColor = System.Drawing.Color.FromArgb((int)_backGrndColorReadOnly);
			}
		}

		public override void applyRetrievedAttribute(IDataBindingSource src, FieldRetrievedEventArgs e) 
		{
            // register default style controller
            src.EditorsHost.FormSupport.DefaultStyles[(e.Control as TextEdit)] = _styleController;
            (e.Control as TextEdit).StyleController = _styleController;
		}

        public override void applyGridColumnPopulation(IDataBindingSource src, GridColumnPopulated e) {
            if (!ReferenceEquals(e.Column, null) && ColumnWidth != -1)
            {
                // set column width
                e.Column.Width = ColumnWidth;
            }
            
                      
        }
    }	
}
