﻿#region Using directives
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using log4net;

using Sharp3D.Math.Core;

using treeDiM.StackBuilder.Engine;
using treeDiM.StackBuilder.Basics;
using treeDiM.StackBuilder.Graphics.Controls;
using treeDiM.StackBuilder.Desktop.Properties;
#endregion

namespace treeDiM.StackBuilder.Desktop
{
    public partial class FormNewAnalysisCylinderCase
        : FormNewAnalysis, IItemBaseFilter
    {

        #region Constructor
        public FormNewAnalysisCylinderCase(Document doc, AnalysisHomo analysis)
            : base(doc, analysis)
        {
            InitializeComponent();
        }
        #endregion

        #region FormNewBase overrides
        public override string ItemDefaultName => Resources.ID_ANALYSIS;
        #endregion

        #region Form override
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cbCylinders.Initialize(_document, this, AnalysisCast?.Content);
            cbCases.Initialize(_document, this, AnalysisCast?.CaseProperties);

            // event handling
            uCtrlLayerList.LayerSelected += OnLayerSelected;
            uCtrlLayerList.RefreshFinished += OnLayerSelected;
            uCtrlLayerList.ButtonSizes = new Size(100, 100);

            checkBoxBestLayersOnly.Checked = Settings.Default.KeepBestSolutions;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Settings.Default.KeepBestSolutions = checkBoxBestLayersOnly.Checked;
        }
        #endregion

        #region FormNewAnalysis override
        public override void OnNext()
        {
            base.OnNext();
            try
            {
                List<LayerDesc> layerDescs = new List<LayerDesc>();
                foreach (ILayer2D layer in uCtrlLayerList.Selected)
                    layerDescs.Add(layer.LayerDescriptor);

                Solution.SetSolver(new LayerSolver());

                if (!(_item is AnalysisCylinderCase analysis))
                {
                    _item = _document.CreateNewAnalysisCylinderCase(
                        ItemName, ItemDescription
                        , SelectedCylinder, SelectedCase
                        , new List<InterlayerProperties>()
                        , BuildConstraintSet()
                        , layerDescs);
                }
                else
                {
                    analysis.ID.SetNameDesc(ItemName, ItemDescription);
                    analysis.Content = SelectedCylinder;
                    analysis.CaseProperties = SelectedCase;
                    analysis.ConstraintSet = BuildConstraintSet();
                    analysis.AddSolution(layerDescs);

                    _document.UpdateAnalysis(analysis);
                }
                Close();
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }
        #endregion

        #region IItemBaseFilter implementation
        public bool Accept(Control ctrl, ItemBase itemBase)
        {
            if (ctrl == cbCylinders)
            {
                return itemBase is CylinderProperties;
            }
            else if (ctrl == cbCases)
            {
                PackableBrick packable = itemBase as PackableBrick;
                return (null != packable) && (packable.IsCase);
            }
            return false;
        }
        #endregion

        #region Event handlers
        private void OnLayerSelected(object sender, EventArgs e)
        {
            try
            {
                ILayer2D[] layers = uCtrlLayerList.Selected;
                UpdateStatus(layers.Length > 0 ? string.Empty : Resources.ID_SELECTATLEASTONELAYOUT);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }
        private void OnInputChanged(object sender, EventArgs e)
        {
            try
            {
                // get cylinder & case
                BoxProperties caseProperties = SelectedCase;
                CylinderProperties cylinder = SelectedCylinder;
                if (null == cylinder || null == caseProperties)
                    return;
                // compute
                LayerSolver solver = new LayerSolver();
                List<Layer2DCyl> layers = solver.BuildLayers(
                    cylinder.RadiusOuter, cylinder.Height
                    , new Vector2D(caseProperties.InsideLength, caseProperties.InsideWidth)
                    , 0.0
                    , BuildConstraintSet()
                    , checkBoxBestLayersOnly.Checked
                    );
                uCtrlLayerList.Packable = cylinder;
                uCtrlLayerList.ContainerHeight = caseProperties.InsideHeight;
                uCtrlLayerList.FirstLayerSelected = true;
                uCtrlLayerList.LayerList = layers.Cast<ILayer2D>().ToList();
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }
        private void OnCylinderChanged(object sender, EventArgs e)
        {
            uCtrlPackable.PackableProperties = SelectedCylinder;
            OnInputChanged(sender, e);
            OnLayerSelected(sender, e);
        }
        #endregion

        #region Private properties
        private CylinderProperties SelectedCylinder => cbCylinders.SelectedType as CylinderProperties;
        private BoxProperties SelectedCase => cbCases.SelectedType as BoxProperties;
        private AnalysisCylinderCase AnalysisCast => _item as AnalysisCylinderCase;
        #endregion

        #region Helpers
        private ConstraintSetCylinderContainer BuildConstraintSet()
        {
            ConstraintSetCylinderContainer constraintSet = new ConstraintSetCylinderContainer(SelectedCase);
            return constraintSet;
        }
        #endregion

        #region Data members
        static readonly ILog _log = LogManager.GetLogger(typeof(FormNewAnalysisCylinderCase));
        #endregion
    }
}
