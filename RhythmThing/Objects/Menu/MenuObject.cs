﻿using RhythmThing.System_Stuff;
using RhythmThing.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RhythmThing.Objects.Menu
{
    class MenuObject : GameObject
    {
        private enum MenuSection
        {
            songSelect,
            optSelect
        }
        private enum optSelections
        {
            options,
            quit
        }

        const int songMenuX = 8;
        const int optionMenuX = 58;

        private float animTime = 0.05f;
        private string animEasing = "easeLinear";
        private int[] lastAnimPoint;
        List<SongContainer> songs;
        private Visual selector;
        private Visual optButton;
        //option button colors
        private ConsoleColor optFront = ConsoleColor.Black;
        private ConsoleColor optBack = ConsoleColor.Yellow;
        private Visual quitButton;
        //quit button colors
        private ConsoleColor quitFront = ConsoleColor.Black;
        private ConsoleColor quitBack = ConsoleColor.Red;
        private ChartInfoVisual chartInfoVisual;
        private int selected = 0;
        private int count = -1;
        private int drawAmount = 8;
        private int currentHighestSelect = 7;//must be same as drawAmount-1
        private int currentLowestSelect = 0;
        private MenuSection menuSection = MenuSection.songSelect;
        private optSelections optionSelected = optSelections.options;
        public override void End()
        {
            //throw new NotImplementedException();
        }

        public override void Start(Game game)
        {
            
            components = new List<Component>();

            string[] chartNames = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "!Content/!Songs"));
            
            songs = new List<SongContainer>();
            for (int i = 0; i < chartNames.Length; i++)
            {
                SongContainer tempcon = new SongContainer(chartNames[i], i);
                game.addGameObject(tempcon);
                songs.Add(tempcon);
                count++;
            }
            songs.Sort((a, b) =>
            {
                return (a.chart.chartInfo.difficulty > b.chart.chartInfo.difficulty) ? 1 : -1;
            });
            selector = new Visual();
            selector.active = true;

            selector.x = 3+5;
            selector.y = 45;
            lastAnimPoint = new int[] { songMenuX, 45 };

            selector.localPositions.Add(new Coords(0, 0, ' ', ConsoleColor.Cyan, ConsoleColor.Cyan));
            selector.localPositions.Add(new Coords(0, 1, ' ', ConsoleColor.Cyan, ConsoleColor.Cyan));
            selector.localPositions.Add(new Coords(0, -1, ' ', ConsoleColor.Cyan, ConsoleColor.Cyan));
            selector.localPositions.Add(new Coords(32, 0, ' ', ConsoleColor.Cyan, ConsoleColor.Cyan));
            selector.localPositions.Add(new Coords(32, 1, ' ', ConsoleColor.Cyan, ConsoleColor.Cyan));
            selector.localPositions.Add(new Coords(32, -1, ' ', ConsoleColor.Cyan, ConsoleColor.Cyan));
            for (int i = 0; i < 33; i++)
            {
                selector.localPositions.Add(new Coords(i, 2, ' ', ConsoleColor.Cyan, ConsoleColor.Cyan));
                selector.localPositions.Add(new Coords(i, -2, ' ', ConsoleColor.Cyan, ConsoleColor.Cyan));
            }
            components.Add(selector);
            chartInfoVisual = new ChartInfoVisual(songs[selected].chart.chartInfo);
            songs[selected].OutAnim();
            game.addGameObject(chartInfoVisual);


            //draw the option visual
            optButton = new Visual();
            optButton.active = true;
            char[] optText = "Options".ToCharArray();
            optButton.x = 60;
            optButton.y = 13;
            for (int i = -1; i < 30; i++)
            {
                optButton.localPositions.Add(new Coords(i, 1, ' ', optFront, optBack));
                optButton.localPositions.Add(new Coords(i, 0, ' ', optFront, optBack));
                optButton.localPositions.Add(new Coords(i, -1, ' ', optFront, optBack));
            }
            for (int i = 0; i < optText.Length; i++)
            {
                optButton.localPositions.Add(new Coords(i, 0, optText[i], optFront, optBack));
            }
            components.Add(optButton);

            //draw the quit visual
            quitButton = new Visual();
            quitButton.active = true;
            char[] quitText = "Quit".ToCharArray();
            quitButton.x = 60;
            quitButton.y = 8;

            for (int i = -1; i < 30; i++)
            {
                quitButton.localPositions.Add(new Coords(i, 1, ' ', quitFront, quitBack));
                quitButton.localPositions.Add(new Coords(i, 0, ' ', quitFront, quitBack));
                quitButton.localPositions.Add(new Coords(i, -1, ' ', quitFront, quitBack));
            }
            for (int i = 0; i < quitText.Length; i++)
            {
                quitButton.localPositions.Add(new Coords(i, 0, quitText[i], quitFront, quitBack));
            }
            components.Add(quitButton);

            DrawFromPoint(0);
        }

        //should only ever be called from a safe index. otherwise I fucked up elsewhere lo
        void DrawFromPoint(int topmost)
        {
            //reset
            foreach(SongContainer songContainer in songs)
            {
                songContainer.visual.active = false;
            }
            //draw starting from point, draw drawamount or limit
            if(songs.Count >= drawAmount)
            {
                for (int i = topmost; i < drawAmount+topmost; i++)
                {
                    songs[i].visual.active = true;
                    songs[i].AnimTo(45 + ((i-topmost) * -5));
                }
            } else
            {
                for (int i = 0; i < songs.Count; i++)
                {
                    songs[i].visual.active = true;
                }
            }

        }

        public override void Update(double time, Game game)
        {
            if (menuSection == MenuSection.songSelect)
            {
                if (game.input.ButtonStates[Input.ButtonKind.Down] == Input.ButtonState.Press)
                {
                    songs[selected].InAnim();
                    if (selected < count)
                    {
                        selected++;
                        if (selected > currentHighestSelect)
                        {
                            currentLowestSelect++;
                            currentHighestSelect++;
                            DrawFromPoint(currentLowestSelect);
                        }
                        else
                        {

                            moveSelector(lastAnimPoint[0], lastAnimPoint[1] - 5);
                        }


                    }
                    else
                    {
                        moveSelector(lastAnimPoint[0], 45);
                        selected = 0;
                        DrawFromPoint(0);
                        currentLowestSelect = 0;
                        currentHighestSelect = drawAmount - 1;
                    }
                    chartInfoVisual.UpdateChart(songs[selected].chart.chartInfo);
                    songs[selected].OutAnim();
                }
                if (game.input.ButtonStates[Input.ButtonKind.Up] == Input.ButtonState.Press)
                {
                    songs[selected].InAnim();
                    if (selected > 0)
                    {
                        selected--;
                        if (selected < currentLowestSelect)
                        {
                            currentLowestSelect--;
                            currentHighestSelect--;
                            DrawFromPoint(currentLowestSelect);
                        }
                        else
                        {
                            
                            moveSelector(lastAnimPoint[0], lastAnimPoint[1] + 5);
                        }

                    }
                    else
                    {
                        if (songs.Count > drawAmount)
                        {
                            moveSelector(lastAnimPoint[0], (45 + ((drawAmount - 1) * -5)));
                            currentLowestSelect = (count - (drawAmount - 1));

                        }
                        else
                        {
                            moveSelector(lastAnimPoint[0], 45 + ((count) * -5));
                            
                            currentLowestSelect = 0;
                        }
                        selected = count;
                        DrawFromPoint(count - (drawAmount - 1));
                        currentHighestSelect = count;
                    }
                    chartInfoVisual.UpdateChart(songs[selected].chart.chartInfo);
                    songs[selected].OutAnim();
                }

                //a little out animation!
                

                if (game.input.ButtonStates[Input.ButtonKind.Confirm] == Input.ButtonState.Press)
                {
                    game.ChartToLoad = songs[selected].chartName;
                    game.sceneManager.loadScene(1);
                }
                if (game.input.ButtonStates[Input.ButtonKind.Left] == Input.ButtonState.Press || game.input.ButtonStates[Input.ButtonKind.Right] == Input.ButtonState.Press)
                {
                    menuSection = MenuSection.optSelect;
                    songs[selected].InAnim();
                    moveSelector(optionMenuX, 13);

                }
            } else if (menuSection == MenuSection.optSelect)
            {
                //else if is so juuuust in case, switching cant be at the same time as trying to press one of these
                if (game.input.ButtonStates[Input.ButtonKind.Left] == Input.ButtonState.Press || game.input.ButtonStates[Input.ButtonKind.Right] == Input.ButtonState.Press)
                {
                    menuSection = MenuSection.songSelect;
                    //calculates where selector should be
                    moveSelector(songMenuX,(45 - ((selected - currentLowestSelect) * 5)));
                    optionSelected = optSelections.options;
                    songs[selected].OutAnim();

                    //this line can check for either right now as there is only two options. I dont see any case where there would be more.
                } else if (game.input.ButtonStates[Input.ButtonKind.Up] == Input.ButtonState.Press || game.input.ButtonStates[Input.ButtonKind.Down] == Input.ButtonState.Press)
                {
                    if (optionSelected == optSelections.options)
                    {
                        optionSelected = optSelections.quit;
                        moveSelector(lastAnimPoint[0], lastAnimPoint[1] - 5);
                    } else if(optionSelected == optSelections.quit)
                    {
                        optionSelected = optSelections.options;
                        moveSelector(lastAnimPoint[0], lastAnimPoint[1] + 5);
                    }
                } else if(game.input.ButtonStates[Input.ButtonKind.Confirm] == Input.ButtonState.Press)
                {
                    //follow through with the selectiooooon
                    if(optionSelected == optSelections.quit)
                    {
                        //THIS SHOULD BE THE ONLY PLACE IN THE CODE WHERE THIS LINE IS EVER WRITTEN. IF ITS ANYWHERE ELSE BAD THINGS COULD HAPPEN
                        Game.gameLoopLives = false;
                        game.running = false;
                    } else if(optionSelected == optSelections.options)
                    {
                        game.sceneManager.loadScene(4);
                    }
                }
                

            }

        }

        private void moveSelector(int x, int y)
        {
            selector.ClearAnims();
            int[] point2 = new int[] { x, y };
            selector.Animate(lastAnimPoint, point2, animEasing, animTime,true);
            lastAnimPoint = point2;
        }
    }
}
