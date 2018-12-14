using System;
using System.Collections.Generic;
using System.Drawing;
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

        public GameModeRacer() : base("spritesheet.high.png")
        {
            _random = new Random();
            _raceStartTime = DateTime.Now;

            Init();
        }

        public override void Render(Game game, Graphics g)
        {
            ClearScreen(g, "#dc9");
            UpdateCarState();
            DrawBackground(g, -_player.PositionX);
            RenderRoad(game, g);
            DrawCar(g);
            DrawHud(game, g);
            CheckForCompletion(game);
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
                            Type = SpriteType.Rock,
                            Position = -0.55,
                        };
                    }
                    else
                    {
                        if (_random.NextDouble() < 0.05)
                        {
                            zoneSprite = new Sprite
                            {
                                Type = SpriteType.Tree,
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

        private void RenderRoad(Game game, Graphics g)
        {
            // TODO
        }

        private void CheckForCompletion(Game game)
        {
            _absoluteIndex = Math.Floor(_player.Position / _roadParams.RoadSegmentSize);
            if (_absoluteIndex >= _roadParams.Length - game.DepthOfField - 1)
            {
                game.SetGameMode(GameMode.Complete);
            }
        }

        private void DrawCar(Graphics g)
        {
            DrawImage(g, _player.Sprite.Location, _player.Sprite.X, _player.Sprite.Y, 1);
        }

        private void DrawHud(Game game, Graphics g)
        {
            var percent = (int)(Math.Round(_absoluteIndex / (_roadParams.Length - game.DepthOfField) * 100));
            DrawString(g, $"{percent}%", 287, 1);

            var diff = DateTime.Now - _raceStartTime;
            var currentTimeString = diff.ToString(@"mm\:ss\:ff");
            DrawString(g, currentTimeString, 1, 1);

            var speed = Math.Round(_player.Speed / _player.MaxSpeed * 200);
            DrawString(g, $"{speed}mph", 1, 10);
        }
    }
}
