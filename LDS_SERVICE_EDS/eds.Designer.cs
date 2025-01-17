namespace LDS_SERVICE_EDS
{
    partial class Lds_service_eds
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.ProcesoConteo = new System.Timers.Timer();
            this.LogEventos = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.ProcesoConteo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogEventos)).BeginInit();
            // 
            // ProcesoConteo
            // 
            this.ProcesoConteo.Enabled = true;
            this.ProcesoConteo.Interval = 500D;
            this.ProcesoConteo.Elapsed += new System.Timers.ElapsedEventHandler(this.ProcesoConteo_Elapsed);
            // 
            // LogEventos
            // 
            this.LogEventos.EntryWritten += new System.Diagnostics.EntryWrittenEventHandler(this.eventLog1_EntryWritten);
            // 
            // Lds_service_eds
            // 
            this.ServiceName = "Lds_service_eds";
            ((System.ComponentModel.ISupportInitialize)(this.ProcesoConteo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogEventos)).EndInit();

        }

        #endregion

        private System.Timers.Timer ProcesoConteo;
        private System.Diagnostics.EventLog LogEventos;
    }
}
