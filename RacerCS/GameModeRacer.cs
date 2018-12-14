﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RacerCS
{
    public class GameModeRacer : GameModeBase
    {
        private Player _player;
        private Random _random;
        private double _absoluteIndex;
        private DateTime _raceStartTime;
        private RoadParameters _roadParams;
        
        private double _lastDelta;
       

        private List<RoadSegment> _road;

        private Rectangle _treeLocation;
        private Rectangle _rockLocation;
        private CarSprite _spriteForward;
        private CarSprite _spriteLeft;
        private CarSprite _spriteRight;

        public GameModeRacer(Game game) : base(game, "spritesheet.high.png")
        {
            _random = new Random();
            _raceStartTime = DateTime.Now;

            Init();
        }

        public override void Render(Graphics g)
        {
            ClearScreen(g, "#dc9");
            UpdateCarState();
            DrawBackground(g, -_player.PositionX);
            RenderRoad(g);
            DrawCar(g);
            DrawHud(g);
            CheckForCompletion();
        }

        private void Init()
        {           
            _treeLocation = new Rectangle(321, 9, 23, 50);
            _rockLocation = new Rectangle(345, 9, 11, 14);

            var carForwardSpriteLocation = new Rectangle(0, 130, 69, 38);
            _spriteForward = new CarSprite(carForwardSpriteLocation, 125, 190);
            
            var carLeftSpriteLocation = new Rectangle(70, 130, 77, 38);
            _spriteLeft = new CarSprite(carLeftSpriteLocation, 117, 190);

            var carRightSpriteLocation = new Rectangle(148, 130, 77, 38);
            _spriteRight = new CarSprite(carRightSpriteLocation, 125, 190);

            _player = new Player
            {
                Position = 10,
                Speed = 0,
                Acceleration = 0.05,
                Deceleration = 0.3,
                Breaking = 0.6,
                Turning = 5.0,
                PositionX = 0,
                MaxSpeed = 15,
                Sprite = _spriteForward,
            };

            GenerateRoad();
        }

        private void GenerateRoad()
        {
            _roadParams = new RoadParameters
            {
                MaxHeight = 900,
                MaxCurve = 400,
                Length = 12,
                Curvy = 0.8,
                Mountiany = 0.8,
                ZoneSize = 250,
                RoadSegmentSize = 5,
                NumberOfSegmentsPerColor = 4,
            };

            _road = new List<RoadSegment>();

            HeightState currentStateHeight = HeightState.Flat;
            HeightState[,] transitionHeights = new HeightState[,]
            {
                { HeightState.Flat, HeightState.Up, HeightState.Down },
                { HeightState.Flat, HeightState.Down, HeightState.Down },
                { HeightState.Flat, HeightState.Up, HeightState.Up },
            };

            CurveState currentStateCurve = CurveState.Straight;
            CurveState[,] transitionCurves = new CurveState[,]
            {
                { CurveState.Straight, CurveState.Left, CurveState.Right },
                { CurveState.Straight, CurveState.Right, CurveState.Right },
                { CurveState.Straight, CurveState.Left, CurveState.Left },
            };

            var currentHeight = 0.0;
            var currentCurve = 0.0;
            var zones = _roadParams.Length;

            while (zones-- > 0)
            {
                // Generate current zone
                var finalHeight = 0.0;
                switch (currentStateHeight)
                {
                    case HeightState.Flat:
                        {
                            finalHeight = 0.0;
                            break;
                        }
                    case HeightState.Up:
                        {
                            finalHeight = _roadParams.MaxHeight * _random.NextDouble();
                            break;
                        }
                    case HeightState.Down:
                        {
                            finalHeight = -_roadParams.MaxHeight * _random.NextDouble();
                            break;
                        }
                }

                var finalCurve = 0.0;
                switch (currentStateCurve)
                {
                    case CurveState.Straight:
                        {
                            finalCurve = 0.0;
                            break;
                        }
                    case CurveState.Left:
                        {
                            finalCurve = -_roadParams.MaxCurve * _random.NextDouble();
                            break;
                        }
                    case CurveState.Right:
                        {
                            finalCurve = _roadParams.MaxCurve * _random.NextDouble();
                            break;
                        }
                }

                for (int i = 0; i < _roadParams.ZoneSize; i++)
                {
                    Sprite zoneSprite = null;

                    // Add a tree
                    if (i % _roadParams.ZoneSize / 4 == 0)
                    {
                        zoneSprite = new Sprite
                        {
                            SourceLocation = _rockLocation,
                            Position = -0.55,
                        };
                    }
                    else
                    {
                        if (_random.NextDouble() < 0.05)
                        {
                            zoneSprite = new Sprite
                            {
                                SourceLocation = _treeLocation,
                                Position = 0.6 + (4 * _random.NextDouble()),
                            };
                            if (_random.NextDouble() < 0.5)
                            {
                                zoneSprite.Position = -zoneSprite.Position;
                            }
                        }
                    }

                    _road.Add(new RoadSegment
                    {
                        Height = currentHeight + finalHeight / 2 * (1 + Math.Sin(i / _roadParams.ZoneSize * Math.PI - Math.PI / 2)),
                        Curve = currentCurve + finalCurve / 2 * (1 + Math.Sin(i / _roadParams.ZoneSize * Math.PI - Math.PI / 2)),
                        ZoneSprite = zoneSprite,
                    });
                }

                currentHeight += finalHeight;
                currentCurve += finalCurve;

                // Find next zone
                if (_random.NextDouble() < _roadParams.Mountiany)
                {
                    currentStateHeight = transitionHeights[(int)currentStateHeight, (int)(1 + Math.Round(_random.NextDouble()))];
                }
                else
                {
                    currentStateHeight = transitionHeights[(int)currentStateHeight, 0];
                }

                if (_random.NextDouble() < _roadParams.Curvy)
                {
                    currentStateCurve = transitionCurves[(int)currentStateCurve, (int)(1 + Math.Round(_random.NextDouble()))];
                }
                else
                {
                    currentStateCurve = transitionCurves[(int)currentStateCurve, 0];
                }
            }
            _roadParams.Length = _roadParams.Length * _roadParams.ZoneSize;
        }

        private void UpdateCarState()
        {
            if (Math.Abs(_lastDelta) > 130)
            {
                if (_player.Speed > 3)
                {
                    _player.Speed -= 0.2;
                }
            }
            else
            {
                // Read acceleration controls
                if (_keys[Keys.W] || _keys[Keys.Up])
                {
                    _player.Speed += _player.Acceleration;
                }
                else if (_keys[Keys.S] || _keys[Keys.Down])
                {
                    _player.Speed -= _player.Breaking;
                }
                else
                {
                    _player.Speed -= _player.Deceleration;
                }
            }

            _player.Speed = Math.Max(_player.Speed, 0);
            _player.Speed = Math.Min(_player.Speed, _player.MaxSpeed);
            _player.Position += _player.Speed;

            // Car Turning            
            if (_keys[Keys.A] || _keys[Keys.Left])
            {
                if (_player.Speed > 0)
                {
                    _player.PositionX -= _player.Turning;
                }
                _player.Sprite = _spriteLeft;
            }
            else if (_keys[Keys.D] || _keys[Keys.Right])
            {
                if (_player.Speed > 0)
                {
                    _player.PositionX += _player.Turning;
                }
                _player.Sprite = _spriteRight;
            }
            else
            {
                _player.Sprite = _spriteForward;
            }
        }

        private void RenderRoad(Graphics g)
        {
            var spriteBuffer = new List<SpriteBufferEntry>();

            _absoluteIndex = Math.Floor(_player.Position / _roadParams.RoadSegmentSize);

            var currentSegmentIndex = (int)((_absoluteIndex - 2) % _roadParams.Length);
            var currentSegmentPosition = (_absoluteIndex - 2) % _roadParams.RoadSegmentSize - _player.Position;
            var currentSegment = _road[currentSegmentIndex];

            var lastProjectedHeight = double.MaxValue;
            var probedDepth = 0.0;
            var counter = _absoluteIndex % (2 * _roadParams.NumberOfSegmentsPerColor); // for alternating color band

            var playerPosSegmentHeight = _road[(int)_absoluteIndex % _road.Count].Height;
            var playerPosNextSegmentHeight = _road[(int)(_absoluteIndex + 1) % _road.Count].Height;
            var playerPosRelative = (_player.Position % _roadParams.RoadSegmentSize) / _roadParams.RoadSegmentSize;
            var playerHeight = Game.CameraHeight + playerPosSegmentHeight + (playerPosNextSegmentHeight - playerPosSegmentHeight) * playerPosRelative;

            var baseOffset = currentSegment.Curve + (_road[(currentSegmentIndex + 1) % _roadParams.Length].Curve - currentSegment.Curve) * playerPosRelative;

            _lastDelta = _player.PositionX - baseOffset * 2;

            var iter = Game.DepthOfField;
            while (iter-- > 0)
            {
                // Next segment
                var nextSegmentIndex = (currentSegmentIndex + 1) % _road.Count;
                var nextSegment = _road[nextSegmentIndex];

                var startProjectedHeight = Math.Floor((playerHeight - currentSegment.Height) * Game.CameraDistance / (Game.CameraDistance + currentSegmentPosition));
                var startScaling = 30 / (Game.CameraDistance + currentSegmentPosition);

                var endProjectedHeight = Math.Floor((playerHeight - nextSegment.Height) * Game.CameraDistance / (Game.CameraDistance + currentSegmentPosition + _roadParams.RoadSegmentSize));
                var endScaling = 30 / (Game.CameraDistance + currentSegmentPosition + _roadParams.RoadSegmentSize);

                var currentHeight = Math.Min(lastProjectedHeight, startProjectedHeight);
                var currentScaling = startScaling;

                if (currentHeight > endProjectedHeight)
                {
                    DrawSegment(g,
                        Game.Height / 2 + currentHeight,
                        currentScaling, 
                        currentSegment.Curve - baseOffset - _lastDelta * currentScaling,
                        Game.Height / 2 + endProjectedHeight,
                        endScaling,
                        nextSegment.Curve - baseOffset - _lastDelta * endScaling,
                        counter < _roadParams.NumberOfSegmentsPerColor, 
                        currentSegmentIndex == 2 || currentSegmentIndex == (_roadParams.Length - Game.DepthOfField));
                }

                if (currentSegment.ZoneSprite != null)
                {
                    spriteBuffer.Add(new SpriteBufferEntry
                    {
                        Y = (int)(Game.Height / 2 + startProjectedHeight),
                        X = (int)(Game.Width / 2 - currentSegment.ZoneSprite.Position * Game.Width * currentScaling + currentSegment.Curve - baseOffset - (_player.PositionX - baseOffset * 2) * currentScaling),
                        YMax = (int)(Game.Height / 2 + lastProjectedHeight),
                        Scale = 2.5 * currentScaling,
                        SourceLocation = currentSegment.ZoneSprite.SourceLocation,
                    });
                }

                lastProjectedHeight = currentHeight;
                probedDepth = currentSegmentPosition;
                currentSegmentIndex = nextSegmentIndex;
                currentSegment = nextSegment;
                currentSegmentPosition += _roadParams.RoadSegmentSize;
                counter = (counter + 1) % (2 * _roadParams.NumberOfSegmentsPerColor);
            }

            foreach (var sprite in spriteBuffer)
            {
                DrawSprite(g, sprite);
            }
        }

        private void DrawSegment(Graphics g, double position1, double scale1, double offset1, double position2, double scale2, double offset2, bool alternate, bool finishStart)
        {
            var grass = alternate ? "#eda" : "#dc9";
            var border = alternate ? "#e00" : "#fff";
            var road = alternate ? "#999" : "#777";
            var lane = alternate ? "#fff" : "#777";

            if (finishStart)
            {
                road = "#fff";
                lane = "#fff";
                border = "#fff";
            }

            // Draw Grass
            var grassBrush = new SolidBrush(ColorTranslator.FromHtml(grass));
            g.FillRectangle(grassBrush, new Rectangle(0, (int)position2, Game.Width, (int)(position1 - position2)));

            // Draw the road
            DrawTrapezoid(g, position1, scale1, offset1, position2, scale2, offset2, -0.5, 0.5, road);

            // Draw the road border
            DrawTrapezoid(g, position1, scale1, offset1, position2, scale2, offset2, -0.5, -0.47, border);
            DrawTrapezoid(g, position1, scale1, offset1, position2, scale2, offset2, 0.47, 0.5, border);

            // Draw the lane line
            DrawTrapezoid(g, position1, scale1, offset1, position2, scale2, offset2, -0.18, -0.15, lane);
            DrawTrapezoid(g, position1, scale1, offset1, position2, scale2, offset2, 0.15, 0.18, lane);
        }

        private void DrawTrapezoid(Graphics g, double position1, double scale1, double offset1, double position2, double scale2, double offset2, double delta1, double delta2, string color)
        {
            var demiWidth = Game.Width / 2;
            var pathBrush = new SolidBrush(ColorTranslator.FromHtml(color));

            var path = new GraphicsPath();
            var point1 = new Point((int)(demiWidth + delta1 * Game.Width * scale1 + offset1), (int)position1);
            var point2 = new Point((int)(demiWidth + delta1 * Game.Width * scale2 + offset2), (int)position2);
            var point3 = new Point((int)(demiWidth + delta2 * Game.Width * scale2 + offset2), (int)position2);
            var point4 = new Point((int)(demiWidth + delta2 * Game.Width * scale1 + offset1), (int)position1);

            path.AddLines(new Point[] { point1, point2, point3, point4 });
            g.FillPath(pathBrush, path);
        }

        private void DrawSprite(Graphics g, SpriteBufferEntry sprite)
        {
            var height = 0;
            var destY = sprite.Y - sprite.SourceLocation.Height * sprite.Scale;
            if (sprite.YMax < sprite.Y)
            {
                height = (int)Math.Min(sprite.SourceLocation.Height * (sprite.YMax - destY) / (sprite.SourceLocation.Height * sprite.Scale), sprite.SourceLocation.Height);
            }
            else
            {
                height = sprite.SourceLocation.Height;
            }

            if (height > 0)
            {
                var source = new Rectangle(sprite.SourceLocation.X, sprite.SourceLocation.Y, sprite.SourceLocation.Width, height);
                var dest = new Rectangle(sprite.X, (int)destY, (int)(sprite.Scale * sprite.SourceLocation.Width), (int)(sprite.Scale * height));
                DrawImage(g, source, dest, 1);
            }
        }

        private void CheckForCompletion()
        {            
            if (_absoluteIndex >= _roadParams.Length - Game.DepthOfField - 1)
            {
                Game.SetGameMode(GameMode.Complete);
            }
        }

        private void DrawCar(Graphics g)
        {
            var dest = new Rectangle(_player.Sprite.X, _player.Sprite.Y, _player.Sprite.Location.Width, _player.Sprite.Location.Height);
            DrawImage(g, _player.Sprite.Location, dest, 1);
        }

        private void DrawHud(Graphics g)
        {
            var percent = (int)(Math.Round(_absoluteIndex / (_roadParams.Length - Game.DepthOfField) * 100));
            DrawString(g, $"{percent}%", 287, 1);

            var diff = DateTime.Now - _raceStartTime;
            var currentTimeString = diff.ToString(@"mm\:ss\:ff");
            DrawString(g, currentTimeString, 1, 1);

            var speed = Math.Round(_player.Speed / _player.MaxSpeed * 200);
            DrawString(g, $"{speed}mph", 1, 10);
        }
    }
}
