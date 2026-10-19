using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectFour : MonoBehaviour
{
    public Button[] columnButtons;   
    public Image[] cells;            
    public TMP_Text status;
    public Button restartButton;
    public int searchDepth = 4;      

    const int R = 6, C = 7;
    const int Empty = 0, Human = 1, AI = 2;
    const int WIN = 100000;

    int[] board;
    bool gameOver;
    List<int[]> lines = new List<int[]>();

    void Start()
    {
        BuildLines();
        for (int i = 0; i < C; i++)
        {
            int col = i;
            columnButtons[i].onClick.AddListener(() => OnHumanMove(col));
        }
        restartButton.onClick.AddListener(Restart);
        Restart();
    }

    void BuildLines()
    {
        int[,] dirs = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };
        for (int r = 0; r < R; r++)
            for (int c = 0; c < C; c++)
                for (int d = 0; d < 4; d++)
                {
                    int er = r + 3 * dirs[d, 0], ec = c + 3 * dirs[d, 1];
                    if (er < 0 || er >= R || ec < 0 || ec >= C) continue;
                    var l = new int[4];
                    for (int k = 0; k < 4; k++)
                        l[k] = (r + k * dirs[d, 0]) * C + (c + k * dirs[d, 1]);
                    lines.Add(l);
                }
    }

    void Restart()
    {
        board = new int[R * C];
        gameOver = false;
        status.text = "Ваш ход";
        UpdateUI();
    }

    void OnHumanMove(int col)
    {
        if (gameOver || board[col] != Empty) return;
        board = Apply(board, col, Human);
        if (CheckEnd()) return;

        board = Apply(board, BestMove(board), AI);
        if (CheckEnd()) return;
        status.text = "Ваш ход";
        UpdateUI();
    }

    bool CheckEnd()
    {
        UpdateUI();
        int w = Winner(board);
        if (w == AI) { status.text = "Победил ИИ"; gameOver = true; }
        else if (w == Human) { status.text = "Вы победили"; gameOver = true; }
        else if (GetMoves(board).Count == 0) { status.text = "Ничья"; gameOver = true; }
        return gameOver;
    }

    void UpdateUI()
    {
        for (int i = 0; i < R * C; i++)
            cells[i].color = board[i] == Human ? Color.red
                           : board[i] == AI ? Color.yellow : Color.white;
    }

    // ---------- Логика игры ----------
    List<int> GetMoves(int[] b)
    {
        var m = new List<int>();
        for (int c = 0; c < C; c++) if (b[c] == Empty) m.Add(c);
        return m;
    }

    int[] Apply(int[] b, int col, int player)
    {
        var nb = (int[])b.Clone();
        for (int r = R - 1; r >= 0; r--)
            if (nb[r * C + col] == Empty) { nb[r * C + col] = player; break; }
        return nb;
    }

    int Winner(int[] b)
    {
        foreach (var l in lines)
        {
            int a = b[l[0]];
            if (a != Empty && a == b[l[1]] && a == b[l[2]] && a == b[l[3]]) return a;
        }
        return 0;
    }

    // ---------- Оценка ----------
    int Evaluate(int[] b)
    {
        int w = Winner(b);
        if (w == AI) return WIN;
        if (w == Human) return -WIN;

        int score = 0;
        // контроль центра
        for (int r = 0; r < R; r++)
        {
            int v = b[r * C + 3];
            if (v == AI) score += 3;
            else if (v == Human) score -= 3;
        }
        // окна по 4 клетки
        foreach (var l in lines)
        {
            int ai = 0, hu = 0, em = 0;
            foreach (int i in l)
            {
                if (b[i] == AI) ai++;
                else if (b[i] == Human) hu++;
                else em++;
            }
            if (ai == 3 && em == 1) score += 100;
            else if (ai == 2 && em == 2) score += 10;
            if (hu == 3 && em == 1) score -= 120;  
            else if (hu == 2 && em == 2) score -= 10;
        }
        return score;
    }

    // ---------- Minimax (depth - сколько ходов осталось) ----------
    int Minimax(int[] b, int depth, bool isMaximizing)
    {
        int score = Evaluate(b);
        if (score >= WIN) return score + depth;   
        if (score <= -WIN) return score - depth;
        var moves = GetMoves(b);
        if (moves.Count == 0) return 0;            // ничья
        if (depth == 0) return score;              

        int best = isMaximizing ? int.MinValue : int.MaxValue;
        foreach (int m in moves)
        {
            int s = Minimax(Apply(b, m, isMaximizing ? AI : Human), depth - 1, !isMaximizing);
            best = isMaximizing ? Mathf.Max(best, s) : Mathf.Min(best, s);
        }
        return best;
    }

    int BestMove(int[] b)
    {
        int bestScore = int.MinValue, bestMove = -1;
        foreach (int m in GetMoves(b))
        {
            int s = Minimax(Apply(b, m, AI), searchDepth - 1, false);
            Debug.Log($"Колонка {m}: оценка {s}");   
            if (s > bestScore || (s == bestScore && Mathf.Abs(m - 3) < Mathf.Abs(bestMove - 3)))
            { bestScore = s; bestMove = m; }
        }
        return bestMove;
    }
}