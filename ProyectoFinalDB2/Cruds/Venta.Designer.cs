namespace ProyectoFinalDB2.Cruds
{
    partial class Venta
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbmlote = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbcliente = new System.Windows.Forms.ComboBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.label5 = new System.Windows.Forms.Label();
            this.btneliminar = new System.Windows.Forms.Button();
            this.btnmodificar = new System.Windows.Forms.Button();
            this.btnguardar = new System.Windows.Forms.Button();
            this.txtproyecto = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbempleado = new System.Windows.Forms.ComboBox();
            this.cmbtipo = new System.Windows.Forms.ComboBox();
            this.numprima = new System.Windows.Forms.NumericUpDown();
            this.numplazo = new System.Windows.Forms.NumericUpDown();
            this.numinteres = new System.Windows.Forms.NumericUpDown();
            this.cmbaval = new System.Windows.Forms.ComboBox();
            this.btnlimpiar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numprima)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numplazo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numinteres)).BeginInit();
            this.SuspendLayout();
            // 
            // cbmlote
            // 
            this.cbmlote.FormattingEnabled = true;
            this.cbmlote.Location = new System.Drawing.Point(281, 20);
            this.cbmlote.Name = "cbmlote";
            this.cbmlote.Size = new System.Drawing.Size(251, 24);
            this.cbmlote.TabIndex = 80;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(79, 386);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(81, 16);
            this.label9.TabIndex = 79;
            this.label9.Text = "Beneficiario:";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(79, 335);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 16);
            this.label8.TabIndex = 78;
            this.label8.Text = "Aval:";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(79, 290);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(50, 16);
            this.label7.TabIndex = 77;
            this.label7.Text = "Interes:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(87, 247);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 16);
            this.label6.TabIndex = 76;
            this.label6.Text = "Plazo";
            // 
            // cmbcliente
            // 
            this.cmbcliente.FormattingEnabled = true;
            this.cmbcliente.Location = new System.Drawing.Point(281, 64);
            this.cmbcliente.Name = "cmbcliente";
            this.cmbcliente.Size = new System.Drawing.Size(251, 24);
            this.cmbcliente.TabIndex = 75;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(620, 247);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(244, 275);
            this.dataGridView1.TabIndex = 74;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(86, 205);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 16);
            this.label5.TabIndex = 73;
            this.label5.Text = "Prima:";
            // 
            // btneliminar
            // 
            this.btneliminar.Location = new System.Drawing.Point(620, 94);
            this.btneliminar.Name = "btneliminar";
            this.btneliminar.Size = new System.Drawing.Size(125, 43);
            this.btneliminar.TabIndex = 72;
            this.btneliminar.Text = "Eliminar";
            this.btneliminar.UseVisualStyleBackColor = true;
            // 
            // btnmodificar
            // 
            this.btnmodificar.Location = new System.Drawing.Point(770, 90);
            this.btnmodificar.Name = "btnmodificar";
            this.btnmodificar.Size = new System.Drawing.Size(125, 43);
            this.btnmodificar.TabIndex = 71;
            this.btnmodificar.Text = "Modificar";
            this.btnmodificar.UseVisualStyleBackColor = true;
            // 
            // btnguardar
            // 
            this.btnguardar.Location = new System.Drawing.Point(620, 20);
            this.btnguardar.Name = "btnguardar";
            this.btnguardar.Size = new System.Drawing.Size(125, 43);
            this.btnguardar.TabIndex = 70;
            this.btnguardar.Text = "Guardar";
            this.btnguardar.UseVisualStyleBackColor = true;
            // 
            // txtproyecto
            // 
            this.txtproyecto.Location = new System.Drawing.Point(274, 386);
            this.txtproyecto.Name = "txtproyecto";
            this.txtproyecto.Size = new System.Drawing.Size(258, 22);
            this.txtproyecto.TabIndex = 66;
            this.txtproyecto.TextChanged += new System.EventHandler(this.txtproyecto_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(86, 163);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 16);
            this.label4.TabIndex = 65;
            this.label4.Text = "Tipo:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(86, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 16);
            this.label3.TabIndex = 64;
            this.label3.Text = "Empleado:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(86, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 16);
            this.label2.TabIndex = 63;
            this.label2.Text = "Lote:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(86, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 16);
            this.label1.TabIndex = 62;
            this.label1.Text = "Cliente: ";
            // 
            // cmbempleado
            // 
            this.cmbempleado.FormattingEnabled = true;
            this.cmbempleado.Location = new System.Drawing.Point(281, 117);
            this.cmbempleado.Name = "cmbempleado";
            this.cmbempleado.Size = new System.Drawing.Size(251, 24);
            this.cmbempleado.TabIndex = 85;
            // 
            // cmbtipo
            // 
            this.cmbtipo.FormattingEnabled = true;
            this.cmbtipo.Location = new System.Drawing.Point(281, 161);
            this.cmbtipo.Name = "cmbtipo";
            this.cmbtipo.Size = new System.Drawing.Size(251, 24);
            this.cmbtipo.TabIndex = 84;
            // 
            // numprima
            // 
            this.numprima.Location = new System.Drawing.Point(280, 206);
            this.numprima.Name = "numprima";
            this.numprima.Size = new System.Drawing.Size(251, 22);
            this.numprima.TabIndex = 86;
            // 
            // numplazo
            // 
            this.numplazo.Location = new System.Drawing.Point(281, 245);
            this.numplazo.Name = "numplazo";
            this.numplazo.Size = new System.Drawing.Size(251, 22);
            this.numplazo.TabIndex = 87;
            // 
            // numinteres
            // 
            this.numinteres.Location = new System.Drawing.Point(280, 290);
            this.numinteres.Name = "numinteres";
            this.numinteres.Size = new System.Drawing.Size(251, 22);
            this.numinteres.TabIndex = 88;
            // 
            // cmbaval
            // 
            this.cmbaval.FormattingEnabled = true;
            this.cmbaval.Location = new System.Drawing.Point(281, 335);
            this.cmbaval.Name = "cmbaval";
            this.cmbaval.Size = new System.Drawing.Size(251, 24);
            this.cmbaval.TabIndex = 89;
            // 
            // btnlimpiar
            // 
            this.btnlimpiar.Location = new System.Drawing.Point(770, 20);
            this.btnlimpiar.Name = "btnlimpiar";
            this.btnlimpiar.Size = new System.Drawing.Size(125, 43);
            this.btnlimpiar.TabIndex = 113;
            this.btnlimpiar.Text = "Limpiar";
            this.btnlimpiar.UseVisualStyleBackColor = true;
            // 
            // Venta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(916, 542);
            this.Controls.Add(this.btnlimpiar);
            this.Controls.Add(this.cmbaval);
            this.Controls.Add(this.numinteres);
            this.Controls.Add(this.numplazo);
            this.Controls.Add(this.numprima);
            this.Controls.Add(this.cmbempleado);
            this.Controls.Add(this.cmbtipo);
            this.Controls.Add(this.cbmlote);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cmbcliente);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btneliminar);
            this.Controls.Add(this.btnmodificar);
            this.Controls.Add(this.btnguardar);
            this.Controls.Add(this.txtproyecto);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Venta";
            this.Text = "Venta";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numprima)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numplazo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numinteres)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox cbmlote;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbcliente;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btneliminar;
        private System.Windows.Forms.Button btnmodificar;
        private System.Windows.Forms.Button btnguardar;
        private System.Windows.Forms.TextBox txtproyecto;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbempleado;
        private System.Windows.Forms.ComboBox cmbtipo;
        private System.Windows.Forms.NumericUpDown numprima;
        private System.Windows.Forms.NumericUpDown numplazo;
        private System.Windows.Forms.NumericUpDown numinteres;
        private System.Windows.Forms.ComboBox cmbaval;
        private System.Windows.Forms.Button btnlimpiar;
    }
}