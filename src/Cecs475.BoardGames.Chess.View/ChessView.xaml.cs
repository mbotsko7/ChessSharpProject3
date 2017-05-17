﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cecs475.BoardGames.Chess.View {
    /// <summary>
    /// Interaction logic for ChessView.xaml
    /// </summary>
    public partial class ChessView : UserControl {
        public ChessView() {
            InitializeComponent();
        }

        public ChessViewModel Model => FindResource("ViewModel") as ChessViewModel;

        private void Border_MouseEnter(object sender, MouseEventArgs e) {
            var border = sender as Border;
            var square = border?.DataContext as ChessSquare;

            if (square == null) return;

            var selectedPiece = Model?.Squares.FirstOrDefault(s => s.IsSelected);

            // If we have a selected piece, limit which squares change color
            if (selectedPiece != null) {
                if (!Model.PossibleMoves.Contains(square.Position)) return;
                square.IsHovered = true;
            } else {
                square.IsHovered = true;
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e) {
            var border = sender as Border;

            var square = border?.DataContext as ChessSquare;
            if (square != null)
                square.IsHovered = false;
        }

        private async void Border_MouseUp(object sender, MouseButtonEventArgs e) {
            var border = sender as Border;
            var square = border?.DataContext as ChessSquare;

            // Null check
            if (square == null || Model == null) return;

            var selectedPiece = Model.Squares.FirstOrDefault(s => s.IsSelected);

            // Highlight if no piece or a different piece is selected
            if (square.Piece.Player == Model.CurrentPlayer && square != selectedPiece)
                square.IsSelected = true;
            
            if (selectedPiece == null) return;
            
            // If the move is invalid, we're done
            if (!square.IsHovered) {
                selectedPiece.IsSelected = false;
                return;
            }

            // Otherwise, apply the move and clear any other flags
            await Model.ApplyMove(square.Position);
            square.IsHovered = false;
            selectedPiece.IsSelected = false;
        }
    }
}