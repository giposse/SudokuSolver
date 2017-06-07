namespace SudokuSolverApp
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.ButtonEditGame = new System.Windows.Forms.Button();
            this.ButtonClearAll = new System.Windows.Forms.Button();
            this.ButtonEndEdit = new System.Windows.Forms.Button();
            this.ButtonSolve = new System.Windows.Forms.Button();
            this.ButtonUndoAll = new System.Windows.Forms.Button();
            this.ButtonRedoAll = new System.Windows.Forms.Button();
            this.ButtonStepForward = new System.Windows.Forms.Button();
            this.ButtonStepBack = new System.Windows.Forms.Button();
            this.TextBoxRuleInformation = new System.Windows.Forms.TextBox();
            this.ListBoxDemoGames = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.UIGameBoard = new SudokuSolverApp.UIGameBoard();
            this.SuspendLayout();
            // 
            // ButtonEditGame
            // 
            this.ButtonEditGame.BackColor = System.Drawing.Color.PaleGreen;
            this.ButtonEditGame.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonEditGame.BackgroundImage")));
            this.ButtonEditGame.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ButtonEditGame.Location = new System.Drawing.Point(12, 27);
            this.ButtonEditGame.Name = "ButtonEditGame";
            this.ButtonEditGame.Size = new System.Drawing.Size(28, 28);
            this.ButtonEditGame.TabIndex = 7;
            this.ButtonEditGame.UseVisualStyleBackColor = false;
            this.ButtonEditGame.Click += new System.EventHandler(this.ButtonEditGame_Click);
            // 
            // ButtonClearAll
            // 
            this.ButtonClearAll.BackColor = System.Drawing.Color.PaleGreen;
            this.ButtonClearAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonClearAll.BackgroundImage")));
            this.ButtonClearAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ButtonClearAll.Location = new System.Drawing.Point(393, 27);
            this.ButtonClearAll.Name = "ButtonClearAll";
            this.ButtonClearAll.Size = new System.Drawing.Size(28, 28);
            this.ButtonClearAll.TabIndex = 8;
            this.ButtonClearAll.UseVisualStyleBackColor = false;
            this.ButtonClearAll.Click += new System.EventHandler(this.OnClearAllCells);
            // 
            // ButtonEndEdit
            // 
            this.ButtonEndEdit.BackColor = System.Drawing.Color.PaleGreen;
            this.ButtonEndEdit.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonEndEdit.BackgroundImage")));
            this.ButtonEndEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ButtonEndEdit.Location = new System.Drawing.Point(12, 27);
            this.ButtonEndEdit.Name = "ButtonEndEdit";
            this.ButtonEndEdit.Size = new System.Drawing.Size(28, 28);
            this.ButtonEndEdit.TabIndex = 9;
            this.ButtonEndEdit.UseVisualStyleBackColor = false;
            this.ButtonEndEdit.Visible = false;
            this.ButtonEndEdit.Click += new System.EventHandler(this.ButtonEndEdit_Click);
            // 
            // ButtonSolve
            // 
            this.ButtonSolve.BackColor = System.Drawing.Color.Yellow;
            this.ButtonSolve.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonSolve.BackgroundImage")));
            this.ButtonSolve.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ButtonSolve.Location = new System.Drawing.Point(194, 7);
            this.ButtonSolve.Name = "ButtonSolve";
            this.ButtonSolve.Size = new System.Drawing.Size(48, 48);
            this.ButtonSolve.TabIndex = 10;
            this.ButtonSolve.UseVisualStyleBackColor = false;
            this.ButtonSolve.Click += new System.EventHandler(this.OnSolvePuzzle_Click);
            // 
            // ButtonUndoAll
            // 
            this.ButtonUndoAll.BackColor = System.Drawing.Color.Transparent;
            this.ButtonUndoAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonUndoAll.BackgroundImage")));
            this.ButtonUndoAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ButtonUndoAll.Location = new System.Drawing.Point(441, 9);
            this.ButtonUndoAll.Name = "ButtonUndoAll";
            this.ButtonUndoAll.Size = new System.Drawing.Size(40, 40);
            this.ButtonUndoAll.TabIndex = 11;
            this.ButtonUndoAll.UseVisualStyleBackColor = false;
            this.ButtonUndoAll.Click += new System.EventHandler(this.ButtonUndoAll_Click);
            // 
            // ButtonRedoAll
            // 
            this.ButtonRedoAll.BackColor = System.Drawing.Color.Transparent;
            this.ButtonRedoAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonRedoAll.BackgroundImage")));
            this.ButtonRedoAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ButtonRedoAll.Location = new System.Drawing.Point(624, 9);
            this.ButtonRedoAll.Name = "ButtonRedoAll";
            this.ButtonRedoAll.Size = new System.Drawing.Size(40, 40);
            this.ButtonRedoAll.TabIndex = 12;
            this.ButtonRedoAll.UseVisualStyleBackColor = false;
            this.ButtonRedoAll.Click += new System.EventHandler(this.ButtonRedoAll_Click);
            // 
            // ButtonStepForward
            // 
            this.ButtonStepForward.BackColor = System.Drawing.Color.Transparent;
            this.ButtonStepForward.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonStepForward.BackgroundImage")));
            this.ButtonStepForward.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ButtonStepForward.Location = new System.Drawing.Point(578, 9);
            this.ButtonStepForward.Name = "ButtonStepForward";
            this.ButtonStepForward.Size = new System.Drawing.Size(40, 40);
            this.ButtonStepForward.TabIndex = 13;
            this.ButtonStepForward.UseVisualStyleBackColor = false;
            this.ButtonStepForward.Click += new System.EventHandler(this.ButtonStepForward_Click);
            // 
            // ButtonStepBack
            // 
            this.ButtonStepBack.BackColor = System.Drawing.Color.Transparent;
            this.ButtonStepBack.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonStepBack.BackgroundImage")));
            this.ButtonStepBack.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ButtonStepBack.Location = new System.Drawing.Point(487, 9);
            this.ButtonStepBack.Name = "ButtonStepBack";
            this.ButtonStepBack.Size = new System.Drawing.Size(40, 40);
            this.ButtonStepBack.TabIndex = 14;
            this.ButtonStepBack.UseVisualStyleBackColor = false;
            this.ButtonStepBack.Click += new System.EventHandler(this.ButtonStepBack_Click);
            // 
            // TextBoxRuleInformation
            // 
            this.TextBoxRuleInformation.BackColor = System.Drawing.Color.WhiteSmoke;
            this.TextBoxRuleInformation.Font = new System.Drawing.Font("Verdana", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxRuleInformation.ForeColor = System.Drawing.Color.Indigo;
            this.TextBoxRuleInformation.Location = new System.Drawing.Point(441, 55);
            this.TextBoxRuleInformation.Multiline = true;
            this.TextBoxRuleInformation.Name = "TextBoxRuleInformation";
            this.TextBoxRuleInformation.ReadOnly = true;
            this.TextBoxRuleInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextBoxRuleInformation.Size = new System.Drawing.Size(223, 271);
            this.TextBoxRuleInformation.TabIndex = 15;
            this.TextBoxRuleInformation.Text = "Rule data here";
            // 
            // ListBoxDemoGames
            // 
            this.ListBoxDemoGames.FormattingEnabled = true;
            this.ListBoxDemoGames.Location = new System.Drawing.Point(441, 354);
            this.ListBoxDemoGames.Name = "ListBoxDemoGames";
            this.ListBoxDemoGames.Size = new System.Drawing.Size(223, 108);
            this.ListBoxDemoGames.Sorted = true;
            this.ListBoxDemoGames.TabIndex = 16;
            this.ListBoxDemoGames.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListBoxDemoGames_MouseDoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(438, 337);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 17);
            this.label1.TabIndex = 17;
            this.label1.Text = "Demo Games";
            // 
            // UIGameBoard
            // 
            this.UIGameBoard.BackColor = System.Drawing.Color.Black;
            this.UIGameBoard.Location = new System.Drawing.Point(12, 55);
            this.UIGameBoard.Name = "UIGameBoard";
            this.UIGameBoard.Size = new System.Drawing.Size(409, 409);
            this.UIGameBoard.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(676, 481);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ListBoxDemoGames);
            this.Controls.Add(this.TextBoxRuleInformation);
            this.Controls.Add(this.ButtonStepBack);
            this.Controls.Add(this.ButtonStepForward);
            this.Controls.Add(this.ButtonRedoAll);
            this.Controls.Add(this.ButtonUndoAll);
            this.Controls.Add(this.ButtonSolve);
            this.Controls.Add(this.ButtonClearAll);
            this.Controls.Add(this.ButtonEditGame);
            this.Controls.Add(this.UIGameBoard);
            this.Controls.Add(this.ButtonEndEdit);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(692, 520);
            this.MinimumSize = new System.Drawing.Size(692, 520);
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal UIGameBoard UIGameBoard;
        private System.Windows.Forms.Button ButtonEditGame;
        private System.Windows.Forms.Button ButtonClearAll;
        private System.Windows.Forms.Button ButtonEndEdit;
        private System.Windows.Forms.Button ButtonSolve;
        private System.Windows.Forms.Button ButtonUndoAll;
        private System.Windows.Forms.Button ButtonRedoAll;
        private System.Windows.Forms.Button ButtonStepForward;
        private System.Windows.Forms.Button ButtonStepBack;
        private System.Windows.Forms.TextBox TextBoxRuleInformation;
        private System.Windows.Forms.ListBox ListBoxDemoGames;
        private System.Windows.Forms.Label label1;


    }
}

