namespace test_client
{
    partial class Form1
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Wymagana metoda wsparcia projektanta - nie należy modyfikować
        /// zawartość tej metody z edytorem kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.etykieta = new System.Windows.Forms.Label();
            this.button = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.et_id_lek = new System.Windows.Forms.Label();
            this.id_lek = new System.Windows.Forms.TextBox();
            this.et_data_rej = new System.Windows.Forms.Label();
            this.et_id_pac = new System.Windows.Forms.Label();
            this.id_pac = new System.Windows.Forms.TextBox();
            this.data_rej = new System.Windows.Forms.DateTimePicker();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // etykieta
            // 
            this.etykieta.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.etykieta.AutoSize = true;
            this.etykieta.Location = new System.Drawing.Point(54, 77);
            this.etykieta.Name = "etykieta";
            this.etykieta.Size = new System.Drawing.Size(157, 13);
            this.etykieta.TabIndex = 0;
            this.etykieta.Text = "Kliknij, aby dodać nową Wizytę.";
            this.etykieta.Click += new System.EventHandler(this.etykieta_Click);
            // 
            // button
            // 
            this.button.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button.Location = new System.Drawing.Point(95, 109);
            this.button.Name = "button";
            this.button.Size = new System.Drawing.Size(75, 23);
            this.button.TabIndex = 1;
            this.button.Text = "Klik!";
            this.button.UseVisualStyleBackColor = true;
            this.button.Click += new System.EventHandler(this.button_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.40298F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.56717F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.40298F));
            this.tableLayoutPanel1.Controls.Add(this.et_id_lek, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.button, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.etykieta, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.id_lek, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.et_data_rej, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.et_id_pac, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.id_pac, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.data_rej, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(268, 142);
            this.tableLayoutPanel1.TabIndex = 2;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // et_id_lek
            // 
            this.et_id_lek.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.et_id_lek.AutoSize = true;
            this.et_id_lek.Location = new System.Drawing.Point(8, 21);
            this.et_id_lek.Name = "et_id_lek";
            this.et_id_lek.Size = new System.Drawing.Size(35, 13);
            this.et_id_lek.TabIndex = 2;
            this.et_id_lek.Text = "id_lek";
            this.et_id_lek.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // id_lek
            // 
            this.id_lek.Location = new System.Drawing.Point(3, 37);
            this.id_lek.Name = "id_lek";
            this.id_lek.Size = new System.Drawing.Size(45, 20);
            this.id_lek.TabIndex = 3;
            // 
            // et_data_rej
            // 
            this.et_data_rej.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.et_data_rej.AutoSize = true;
            this.et_data_rej.Location = new System.Drawing.Point(110, 21);
            this.et_data_rej.Name = "et_data_rej";
            this.et_data_rej.Size = new System.Drawing.Size(45, 13);
            this.et_data_rej.TabIndex = 4;
            this.et_data_rej.Text = "data_rej";
            // 
            // et_id_pac
            // 
            this.et_id_pac.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.et_id_pac.AutoSize = true;
            this.et_id_pac.Location = new System.Drawing.Point(222, 21);
            this.et_id_pac.Name = "et_id_pac";
            this.et_id_pac.Size = new System.Drawing.Size(39, 13);
            this.et_id_pac.TabIndex = 6;
            this.et_id_pac.Text = "id_pac";
            // 
            // id_pac
            // 
            this.id_pac.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.id_pac.Location = new System.Drawing.Point(218, 37);
            this.id_pac.Name = "id_pac";
            this.id_pac.Size = new System.Drawing.Size(47, 20);
            this.id_pac.TabIndex = 7;
            // 
            // data_rej
            // 
            this.data_rej.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.data_rej.Location = new System.Drawing.Point(54, 37);
            this.data_rej.Name = "data_rej";
            this.data_rej.Size = new System.Drawing.Size(158, 20);
            this.data_rej.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(272, 145);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Dodaj nową Wizytę!";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label etykieta;
        private System.Windows.Forms.Button button;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label et_id_lek;
        private System.Windows.Forms.TextBox id_lek;
        private System.Windows.Forms.Label et_data_rej;
        private System.Windows.Forms.Label et_id_pac;
        private System.Windows.Forms.TextBox id_pac;
        private System.Windows.Forms.DateTimePicker data_rej;
    }
}

