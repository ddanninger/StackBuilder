﻿#region Using directives
using log4net;

using treeDiM.StackBuilder.Basics;
using treeDiM.StackBuilder.Desktop.Properties;
#endregion

namespace treeDiM.StackBuilder.Desktop
{
    public partial class DockContentAnalysisCaseTruck : DockContentAnalysisEdit
    {
        #region Constructor
        public DockContentAnalysisCaseTruck(IDocument doc, AnalysisCaseTruck analysis)
            : base(doc, analysis)
        {
            InitializeComponent();
        }
        #endregion

        #region Override DockContentAnalysisEdit
        public override string GridCaption
        {   get { return Resources.ID_TRUCK; } }
        #endregion

        #region Data members
        static readonly new ILog _log = LogManager.GetLogger(typeof(DockContentAnalysisCaseTruck));
        #endregion
    }
}
