using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RacerCS
{
    public partial class RacerForm : Form
    {
        private Dictionary<Keys, bool> _keys;
        private Image _spriteSheet;

        private DateTime _startTime;
        private double _lastDelta;
        private Random _random;

        private Player _player;
        private RoadParameters _roadParameters;
        private RenderInformation _renderInformation;
        private SpritesheetLocation _carForwardSpriteLocation;
        private SpritesheetLocation _carLeftSpriteLocation;
        private SpritesheetLocation _carRightSpriteLocation;
        private SpritesheetLocation _backgroundSprite;
        private SpritesheetLocation _treeSprite;
        private SpritesheetLocation _rockSprite;
        private SpritesheetLocation _logoSprite;
        private CarSprite _spriteForward;
        private CarSprite _spriteLeft;
        private CarSprite _spriteRight;

        private List<RoadSegment> _road;

        public RacerForm()
        {
            InitializeComponent();

            _keys = new Dictionary<Keys, bool>
            {
                { Keys.W, false },
                { Keys.A, false },
                { Keys.S, false },
                { Keys.D, false },
                { Keys.Up, false },
                { Keys.Down, false },
                { Keys.Left, false },
                { Keys.Right, false },
            };

            _random = new Random();

            Init();
            GenerateRoad();
            LoadSpriteSheet("spritesheet.high.png");
            _startTime = DateTime.Now;
        }

        private void Init()
        {


            _roadParameters = new RoadParameters
            {
                MaxHeight = 900,
                MaxCurve = 400,
                Length = 12,
                Curvy = 0.8,
                Mountiany = 0.8,
                ZoneSize = 250,
            };

            _renderInformation = new RenderInformation
            {
                Width = 320,
                Height = 240,
                DepthOfField = 150,
                CameraDistance = 30,
                CameraHeight = 100,
            };

            _carForwardSpriteLocation = new SpritesheetLocation
            {
                X = 0,
                Y = 130,
                Width = 69,
                Height = 38,
            };

            _carLeftSpriteLocation = new SpritesheetLocation
            {
                X = 70,
                Y = 130,
                Width = 77,
                Height = 38,
            };

            _carRightSpriteLocation = new SpritesheetLocation
            {
                X = 148,
                Y = 130,
                Width = 77,
                Height = 38,
            };

            _backgroundSprite = new SpritesheetLocation
            {
                X = 0,
                Y = 9,
                Width = 320,
                Height = 120,
            };

            _treeSprite = new SpritesheetLocation
            {
                X = 321,
                Y = 9,
                Width = 23,
                Height = 50,
            };

            _rockSprite = new SpritesheetLocation
            {
                X = 345,
                Y = 9,
                Width = 11,
                Height = 14,
            };

            _logoSprite = new SpritesheetLocation
            {
                X = 161,
                Y = 39,
                Width = 115,
                Height = 20,
            };

            _spriteForward = new CarSprite
            {
                Location = _carForwardSpriteLocation,
                X = 125,
                Y = 190,
            };

            _spriteLeft = new CarSprite
            {
                Location = _carLeftSpriteLocation,
                X = 117,
                Y = 190,
            };

            _spriteRight = new CarSprite
            {
                Location = _carRightSpriteLocation,
                X = 125,
                Y = 190,
            };

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

        }

        private void LoadSpriteSheet(string fileName)
        {
            _spriteSheet = Bitmap.FromFile(fileName);
        }

        private void RacerForm_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            Application.Idle += Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            RenderGameFrame(e.Graphics);
        }

        private void RacerForm_KeyDown(object sender, KeyEventArgs e)
        {
            _keys[e.KeyCode] = true;
        }

        private void RacerForm_KeyUp(object sender, KeyEventArgs e)
        {
            _keys[e.KeyCode] = false;
        }
        
        private void GenerateRoad()
        {
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
            var zones = _roadParameters.Length;

            while(zones-- > 0)
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
                            finalHeight = _roadParameters.MaxHeight * _random.NextDouble();
                            break;
                        }
                    case HeightState.Down:
                        {
                            finalHeight = -_roadParameters.MaxHeight * _random.NextDouble();
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
                            finalCurve = -_roadParameters.MaxCurve * _random.NextDouble();
                            break;
                        }
                    case CurveState.Right:
                        {
                            finalCurve = _roadParameters.MaxCurve * _random.NextDouble();
                            break;
                        }
                }

                for (int i = 0; i < _roadParameters.ZoneSize; i++)
                {
                    Sprite zoneSprite = null;

                    // Add a tree
                    if (i % _roadParameters.ZoneSize / 4 == 0)
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
                        Height = currentHeight + finalHeight / 2 * (1 + Math.Sin(i/_roadParameters.ZoneSize * Math.PI - Math.PI / 2)),
                        Curve = currentCurve + finalCurve / 2 * (1 + Math.Sin(i/_roadParameters.ZoneSize * Math.PI - Math.PI / 2)),
                        ZoneSprite = zoneSprite,
                    });
                }

                currentHeight += finalHeight;
                currentCurve += finalCurve;

                // Find next zone
                if (_random.NextDouble() < _roadParameters.Mountiany)
                {
                    currentStateHeight = transitionHeights[(int)currentStateHeight, (int)(1 + Math.Round(_random.NextDouble()))];
                }
                else
                {
                    currentStateHeight = transitionHeights[(int)currentStateHeight, 0];
                }

                if (_random.NextDouble() < _roadParameters.Curvy)
                {
                    currentStateCurve = transitionCurves[(int)currentStateCurve, (int)(1 + Math.Round(_random.NextDouble()))];
                }
                else
                {
                    currentStateCurve = transitionCurves[(int)currentStateCurve, 0];
                }
            }
            _roadParameters.Length = _roadParameters.Length * _roadParameters.ZoneSize;
        }

        private void RenderGameFrame(Graphics g)
        {
            ClearScreen(g);
            UpdateCarState();
            DrawBackground(g, -_player.PositionX);
            RenderRoad(g);
            DrawCar(g);
            DrawHud(g);
        }

        private void ClearScreen(Graphics g)
        {
            g.Clear(ColorTranslator.FromHtml("#dc9"));
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

        private void DrawBackground(Graphics g, double playerPosition)
        {
            var first = playerPosition / 2 % (_backgroundSprite.Width);
            DrawImage(g, _backgroundSprite, first - _backgroundSprite.Width + 1, 0, 1);
            DrawImage(g, _backgroundSprite, first + _backgroundSprite.Width - 1, 0, 1);
            DrawImage(g, _backgroundSprite, first, 0, 1);
        }

        private void RenderRoad(Graphics g)
        {
            // TODO
        }

        private void DrawCar(Graphics g)
        {
            DrawImage(g, _player.Sprite.Location, _player.Sprite.X, _player.Sprite.Y, 1);
        }

        private void DrawHud(Graphics g)
        {
            // TODO
        }
        
        private void DrawImage(Graphics g, SpritesheetLocation image, double x, double y, int scale)
        {
            //void ctx.drawImage(image, sx, sy, sWidth, sHeight, dx, dy, dWidth, dHeight);
            //context.drawImage(spritesheet,  image.x, image.y, image.w, image.h, x, y, scale*image.w, scale*image.h);
            var sourceRect = new RectangleF(image.X, image.Y, image.Width, image.Height);
            g.DrawImage(_spriteSheet, (float)x, (float)y, sourceRect, GraphicsUnit.Pixel);
        }

        private void DrawString(Graphics g, string value, int x, int y)
        {
            value = value.ToUpper();
            var currentX = x;
            for (int i = 0; i < value.Length; i++)
            {
                var sourceRect = new RectangleF((value[i] - 32) * 8, 0, 8, 8);
                g.DrawImage(_spriteSheet, (float)currentX, y, sourceRect, GraphicsUnit.Pixel);
            }
        }
    }
}
