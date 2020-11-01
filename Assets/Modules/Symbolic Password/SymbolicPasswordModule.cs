﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class SymbolicPasswordModule : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio Audio;
    public KMSelectable[] buttons;
    public KMSelectable submitButton;
    public MeshRenderer[] labels;
    public Material[] symbols;
    public Material blankScreen;
    public Material WhiteButton;
    public Material RedButton;

    // This is the arrangement of the symbols as printed in the manual.
    static char[,] symbolTable = new char[,] {
        {'Ϙ', 'Ӭ', '©', 'Ϭ', 'Ψ', 'Ϭ', '¿'},
        {'Ѧ', 'Ϙ', 'Ѽ', '¶', 'ټ', 'Ӭ', '☆'},
        {'ƛ', 'Ͽ', 'Ҩ', 'Ѣ', 'Ѣ', '҂', 'Ϙ'},
        {'Ϟ', 'Ҩ', 'Җ', 'Ѭ', 'Ͼ', 'æ', 'ƛ'},
        {'Ѭ', '☆', 'Ԇ', 'Җ', '¶', 'Ψ', 'Ҩ'},
        {'ϗ', 'ϗ', 'ƛ', '¿', 'Ѯ', 'Ҋ', 'Ӭ'},
        {'Ͽ', '¿', '☆', 'ټ', '★', 'Ω', 'Ѽ'}
    };

    // This is the order in which the materials are stored in ‘symbols’.
    static string characters = "©★☆ټҖΩѬѼϗϬϞѦæԆӬҊѮ¿¶ϾϿΨҨ҂ϘƛѢ";

    char[,] display;
    int x;
    int y;
    int moduleId;
    static int moduleIdCounter = 1;
    bool solved;

    void Start()
    {
        moduleId = moduleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += OnActivate;

        buttons[0].OnInteract += delegate { return RotateVertical(display, buttons[0], 1); };
        buttons[1].OnInteract += delegate { return RotateVertical(display, buttons[1], 1); };
        buttons[2].OnInteract += delegate { return RotateVertical(display, buttons[2], 2); };
        buttons[3].OnInteract += delegate { return RotateVertical(display, buttons[3], 2); };
        buttons[4].OnInteract += delegate { return RotateVertical(display, buttons[4], 3); };
        buttons[5].OnInteract += delegate { return RotateVertical(display, buttons[5], 3); };
        buttons[6].OnInteract += delegate { return RotateHorizontal(display, buttons[6], 0, -1); };
        buttons[7].OnInteract += delegate { return RotateHorizontal(display, buttons[7], 0, 1); };
        buttons[8].OnInteract += delegate { return RotateHorizontal(display, buttons[8], 1, -1); };
        buttons[9].OnInteract += delegate { return RotateHorizontal(display, buttons[9], 1, 1); };

        submitButton.OnInteract += Submit;

        x = Random.Range(0, 5); // x = 0–4
        y = Random.Range(0, 6); // y = 0–5
        Debug.LogFormat("[Symbolic Password #{0}] Position in grid: ({1}, {2})", moduleId, x + 1, y + 1);

        var initialOrder = Enumerable.Range(0, 6).ToArray();
        ShuffleArray(initialOrder);
        display = new char[2, 3];
        for (int i = 0; i < 6; i++)
            display[i / 3, i % 3] = symbolTable[y + initialOrder[i] / 3, x + initialOrder[i] % 3];
        Debug.LogFormat("[Symbolic Password #{0}] Initial display: {1} / {2}", moduleId, new string(Enumerable.Range(0, 3).Select(i => display[0, i]).ToArray()), new string(Enumerable.Range(0, 3).Select(i => display[1, i]).ToArray()));
        Debug.LogFormat("[Symbolic Password #{0}] Solution: {1}", moduleId, new string(Enumerable.Range(0, 6).Select(i => symbolTable[y + i / 3, x + i % 3]).ToArray()).Insert(3, " / "));
    }

    void OnActivate()
    {
        RedrawSymbols();
    }

    private bool RotateVertical(char[,] disp, KMSelectable button, int column)
    {
        Audio.PlaySoundAtTransform("tick", button.transform);
        button.AddInteractionPunch(0.25f);
        RotateVertical(disp, column);
        RedrawSymbols();
        return false;
    }

    private static void RotateVertical(char[,] disp, int column)
    {
        var temp = disp[0, column - 1];
        disp[0, column - 1] = disp[1, column - 1];
        disp[1, column - 1] = temp;
    }

    private bool RotateHorizontal(char[,] disp, KMSelectable button, int line, int direction = 0)
    {
        Audio.PlaySoundAtTransform("tick", button.transform);
        button.AddInteractionPunch(0.25f);
        RotateHorizontal(disp, line, direction);
        RedrawSymbols();
        return false;
    }

    private static void RotateHorizontal(char[,] disp, int line, int direction)
    {
        if (direction == -1)
        {
            var temp = disp[line, 0];
            disp[line, 0] = disp[line, 1];
            disp[line, 1] = disp[line, 2];
            disp[line, 2] = temp;
        }
        else
        {
            var temp = disp[line, 2];
            disp[line, 2] = disp[line, 1];
            disp[line, 1] = disp[line, 0];
            disp[line, 0] = temp;
        }
    }

    void RedrawSymbols()
    {
        for (int i = 0; i < 6; i++)
            labels[i].sharedMaterial = symbols[characters.IndexOf(display[i / 3, i % 3])];
    }

    bool Submit()
    {
        Audio.PlaySoundAtTransform("tick", this.transform);
        GetComponent<KMSelectable>().AddInteractionPunch();
        Debug.LogFormat("[Symbolic Password #{0}] You submitted: {1} / {2}", moduleId, new string(Enumerable.Range(0, 3).Select(i => display[0, i]).ToArray()), new string(Enumerable.Range(0, 3).Select(i => display[1, i]).ToArray()));

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                if (display[j, i] != symbolTable[y + j, x + i])
                {
                    Debug.LogFormat("[Symbolic Password #{0}] Wrong solution. Strike.", moduleId);
                    BombModule.HandleStrike();
                    return false;
                }
            }
        }

        Debug.LogFormat("[Symbolic Password #{0}] Module solved.", moduleId);
        BombModule.HandlePass();
        solved = true;
        return false;
    }

    void ShuffleArray<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int r = Random.Range(0, i);
            T tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
    }

    void SelectGroup(int b1, int b2)
    {
        Debug.LogFormat("[Symbolic Password #{0}] Group <{1},{2}> selected.", moduleId, b1 + 1, b2 + 2);
        buttons[b1].gameObject.GetComponent<Renderer>().sharedMaterial = RedButton;
        buttons[b2].gameObject.GetComponent<Renderer>().sharedMaterial = RedButton;
    }

    void DeselectGroup(int b1, int b2)
    {
        Debug.LogFormat("[Symbolic Password #{0}] Group <{1},{2}> deselected.", moduleId, b1 + 1, b2 + 2);
        buttons[b1].gameObject.GetComponent<Renderer>().sharedMaterial = WhiteButton;
        buttons[b2].gameObject.GetComponent<Renderer>().sharedMaterial = WhiteButton;
    }

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToLowerInvariant();
        var pieces = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (pieces.Length >= 2 && pieces[0] == "cycle")
        {
            var list = new List<KMSelectable>();
            int button;
            for (int i = 1; i < pieces.Length; i++)
            {
                switch (pieces[i])
                {
                    case "l": case "left": list.Add(buttons[0]); break;
                    case "m": case "middle": case "c": case "center": case "centre": list.Add(buttons[2]); break;
                    case "r": case "right": list.Add(buttons[4]); break;

                    case "t":
                    case "top":
                    case "u":
                    case "up":
                    case "upper":
                        if ((i + 1) == pieces.Length)
                            return null;
                        switch (pieces[i + 1])
                        {
                            case "l": case "left": button = 6; break;
                            case "r": case "right": button = 7; break;
                            default: return null;
                        }
                        list.Add(buttons[button]);
                        i++;
                        break;

                    case "tl":
                    case "topleft":
                    case "ul":
                    case "upleft":
                    case "upperleft":
                        list.Add(buttons[6]);
                        break;

                    case "tr":
                    case "topright":
                    case "ur":
                    case "upright":
                    case "upperright":
                        list.Add(buttons[7]);
                        break;

                    case "b":
                    case "bottom":
                    case "d":
                    case "down":
                    case "lower":
                        if ((i + 1) == pieces.Length)
                            return null;
                        switch (pieces[i + 1])
                        {
                            case "l": case "left": button = 8; break;
                            case "r": case "right": button = 9; break;
                            default: return null;
                        }
                        list.Add(buttons[button]);
                        i++;
                        break;

                    case "bl":
                    case "bottomleft":
                    case "dl":
                    case "downleft":
                    case "lowerleft":
                        list.Add(buttons[8]);
                        break;

                    case "br":
                    case "bottomright":
                    case "dr":
                    case "downright":
                    case "lowerright":
                        list.Add(buttons[9]);
                        break;

                    default:
                        return null;
                }
            }
            return list.ToArray();
        }
        else if (pieces.Length == 1 && pieces[0] == "submit")
            return new[] { submitButton };
        else
            return null;
    }

    struct SolverQueueItem
    {
        public char[,] PrevConfiguration;
        public char[,] NextConfiguration;
        public int Button;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (solved)
            yield break;

        var q = new Queue<SolverQueueItem>();
        q.Enqueue(new SolverQueueItem { Button = -1, NextConfiguration = display });
        var already = new Dictionary<string, SolverQueueItem>();
        var solutionKey = Enumerable.Range(0, 6).Select(ix => symbolTable[y + ix / 3, x + ix % 3]).Join("");

        while (q.Count > 0)
        {
            var item = q.Dequeue();
            var str = Enumerable.Range(0, 6).Select(ix => item.NextConfiguration[ix / 3, ix % 3]).Join("");

            if (already.ContainsKey(str))
                continue;
            already[str] = item;

            if (str == solutionKey)
                break;

            var config = (char[,]) item.NextConfiguration.Clone();
            RotateVertical(config, 1);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 0 });
            config = (char[,]) item.NextConfiguration.Clone();
            RotateVertical(config, 2);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 2 });
            config = (char[,]) item.NextConfiguration.Clone();
            RotateVertical(config, 3);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 4 });
            config = (char[,]) item.NextConfiguration.Clone();
            RotateHorizontal(config, 0, -1);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 6 });
            config = (char[,]) item.NextConfiguration.Clone();
            RotateHorizontal(config, 0, 1);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 7 });
            config = (char[,]) item.NextConfiguration.Clone();
            RotateHorizontal(config, 1, -1);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 8 });
            config = (char[,]) item.NextConfiguration.Clone();
            RotateHorizontal(config, 1, 1);
            q.Enqueue(new SolverQueueItem { PrevConfiguration = item.NextConfiguration, NextConfiguration = config, Button = 9 });
        }

        if (!already.ContainsKey(solutionKey))
            throw new InvalidOperationException();

        var buttonPresses = new List<int>();
        var cnf = solutionKey;
        while (cnf != null)
        {
            var item = already[cnf];
            buttonPresses.Add(item.Button);
            cnf = item.PrevConfiguration == null ? null : Enumerable.Range(0, 6).Select(ix => item.PrevConfiguration[ix / 3, ix % 3]).Join("");
        }

        for (int i = buttonPresses.Count - 2; i >= 0; i--)
        {
            buttons[buttonPresses[i]].OnInteract();
            yield return new WaitForSeconds(.1f);
        }

        submitButton.OnInteract();
        yield return new WaitForSeconds(.1f);
    }
}
