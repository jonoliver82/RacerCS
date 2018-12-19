using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private List<RoadZone> _roadZones;

        private Rectangle _cactusSpriteLocation;
        private Rectangle _rockSpriteLocation;
        private Rectangle _carForwardSpriteLocation;
        private Rectangle _carLeftSpriteLocation;
        private Rectangle _carRightSpriteLocation;

        public GameModeRacer(Game game) : base(game, "spritesheet.high.png")
        {
            _random = new Random();
            _raceStartTime = DateTime.Now;

            Init();
        }

        public override void Render(Graphics g)
        {
            ClearScreen(g, GameColors.GRASS);
            UpdateCarState();
            DrawBackground(g, -_player.PositionX);
            RenderRoad(g);
            DrawCar(g);
            DrawHud(g);
            CheckForCompletion();
        }

        private void Init()
        {           
            _cactusSpriteLocation = new Rectangle(321, 9, 23, 50);
            _rockSpriteLocation = new Rectangle(345, 9, 11, 14);
            _carForwardSpriteLocation = new Rectangle(0, 130, 69, 38);
            _carLeftSpriteLocation = new Rectangle(70, 130, 77, 38);
            _carRightSpriteLocation = new Rectangle(148, 130, 77, 38);

            var carX = 125;
            var carY = 190;

            _player = new Player
            {
                Position = 10,
                Speed = 0,
                Acceleration = 0.01,
                Deceleration = 0.01,
                DecelerationOffRoad = 0.5,
                Breaking = 0.6,
                Turning = 0.5,
                PositionX = 0,
                MaxSpeed = 5,
                MaxSpeedOffRoad = 3,
                SpriteSource = _carForwardSpriteLocation,
                SpriteDestination = new Point(carX, carY),
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
                NumberOfZones = 12,
                Curvy = 0.8,
                Mountiany = 0.8,
                NumberOfSegmentsPerZone = 250,
                RoadSegmentSize = 5,
                NumberOfSegmentsPerColor = 4,
                OffRoadOffset = 130,
            };
            _roadParams.Length = _roadParams.NumberOfZones * _roadParams.NumberOfSegmentsPerZone;

            _road = new List<RoadSegment>();
            _roadZones = new List<RoadZone>();

            HeightState currentStateHeight = HeightState.Flat;
            CurveState currentStateCurve = CurveState.Straight;

            var currentHeight = 0.0;
            var currentCurve = 0.0;
            var zones = _roadParams.NumberOfZones;

            while (zones-- > 0)
            {
                var zone = new RoadZone(_roadParams.NumberOfSegmentsPerZone);

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

                zone.AddCurve(currentStateCurve, currentCurve, finalCurve);
                zone.AddHeight(currentStateHeight, currentHeight, finalHeight);

                for (int i = 0; i < _roadParams.NumberOfSegmentsPerZone; i++)
                {
                    var segment = new RoadSegment
                    {
                        Height = zone.StartHeight + (zone.HeightStep * i),
                        Curve = zone.StartCurve + (zone.CurveStep * i),
                        SegmentSprite = SelectSegmentSprite(i)
                    };

                    _road.Add(segment);
                    zone.AddSegment(segment);
                }

                currentHeight = finalHeight;
                currentCurve = finalCurve;

                currentStateHeight = SelectNextZoneHeightTransition();
                currentStateCurve = SelectNextZoneCurveTransition();

                _roadZones.Add(zone);
            }
        }

        private CurveState SelectNextZoneCurveTransition()
        {
            if (_random.NextDouble() > _roadParams.Curvy)
            {
                return CurveState.Straight;
            }
            else
            {
                if (_random.NextDouble() > 0.5)
                {
                    return CurveState.Left;
                }
                else
                {
                    return CurveState.Right;
                }
            }
        }

        private HeightState SelectNextZoneHeightTransition()
        {
            if (_random.NextDouble() < _roadParams.Mountiany)
            {
                if (_random.NextDouble() > 0.5)
                {
                    return HeightState.Up;
                }
                else
                {
                    return HeightState.Down;
                }
            }
            else
            {
                return HeightState.Flat;
            }
        }

        private SegmentSprite SelectSegmentSprite(int segmentPosition)
        {
            SegmentSprite segmentSprite = null;

            // Add a rock or cactus
            if (segmentPosition % _roadParams.NumberOfSegmentsPerZone / 4 == 0)
            {
                segmentSprite = new SegmentSprite
                {
                    SourceLocation = _rockSpriteLocation,
                    Position = -0.55,
                };
            }
            else
            {
                if (_random.NextDouble() < 0.05)
                {
                    segmentSprite = new SegmentSprite
                    {
                        SourceLocation = _cactusSpriteLocation,
                        Position = 0.6 + (4 * _random.NextDouble()),
                    };
                }
            }

            if (segmentSprite != null)
            {
                if (_random.NextDouble() < 0.5)
                {
                    segmentSprite.Position = -segmentSprite.Position;
                }
            }

            return segmentSprite;
        }

        private bool IsOffRoad()
        {
            return Math.Abs(_lastDelta) > _roadParams.OffRoadOffset;
        }

        private void UpdateCarState()
        {
            // If player position is off road then reduce player speed
            if (IsOffRoad())
            {
                if (_player.Speed > _player.MaxSpeedOffRoad)
                {
                    _player.Speed -= _player.DecelerationOffRoad;
                }
            }

            // Check keys
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
                _player.Speed -= IsOffRoad() ? _player.DecelerationOffRoad : _player.Deceleration;
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
                _player.SpriteSource = _carLeftSpriteLocation;
            }
            else if (_keys[Keys.D] || _keys[Keys.Right])
            {
                if (_player.Speed > 0)
                {
                    _player.PositionX += _player.Turning;
                }
                _player.SpriteSource = _carRightSpriteLocation;
            }
            else
            {
                _player.SpriteSource = _carForwardSpriteLocation;
            }
        }

        private void RenderRoad(Graphics g)
        {
            var spriteBuffer = new List<SpriteBufferEntry>();
            _absoluteIndex = Math.Floor(_player.Position / _roadParams.RoadSegmentSize);

            var currentSegmentIndex = (int)((_absoluteIndex - 2) % _roadParams.Length);
            var currentSegmentPosition = ((_absoluteIndex - 2) * _roadParams.RoadSegmentSize) - _player.Position;
            var currentSegment = _road[currentSegmentIndex];

            var lastProjectedHeight = double.MaxValue;
            var probedDepth = 0.0;
            var counter = _absoluteIndex % (2 * _roadParams.NumberOfSegmentsPerColor); // for alternating color band

            var playerPosSegmentHeight = _road[(int)_absoluteIndex % _road.Count].Height;
            var playerPosNextSegmentHeight = _road[(int)(_absoluteIndex + 1) % _road.Count].Height;
            var playerPosRelative = (_player.Position % _roadParams.RoadSegmentSize) / _roadParams.RoadSegmentSize;
            var playerHeight = Game.CameraHeight + playerPosSegmentHeight + (playerPosNextSegmentHeight - playerPosSegmentHeight) * playerPosRelative;

            var baseOffset = currentSegment.Curve + (_road[(currentSegmentIndex + 1) % _roadParams.Length].Curve - currentSegment.Curve) * playerPosRelative;

            _lastDelta = _player.PositionX - (baseOffset * 2);

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
                        (Game.Height / 2) + currentHeight,
                        currentScaling, 
                        currentSegment.Curve - baseOffset - _lastDelta * currentScaling,
                        (Game.Height / 2) + endProjectedHeight,
                        endScaling,
                        nextSegment.Curve - baseOffset - _lastDelta * endScaling,
                        counter < _roadParams.NumberOfSegmentsPerColor, 
                        currentSegmentIndex == 2 || currentSegmentIndex == (_roadParams.Length - Game.DepthOfField));
                }

                if (currentSegment.SegmentSprite != null)
                {
                    spriteBuffer.Add(new SpriteBufferEntry
                    {
                        Y = (int)(Game.Height / 2 + startProjectedHeight),
                        X = (int)(Game.Width / 2 - currentSegment.SegmentSprite.Position * Game.Width * currentScaling + currentSegment.Curve - baseOffset - (_player.PositionX - (baseOffset * 2)) * currentScaling),
                        YMax = (int)(Game.Height / 2 + lastProjectedHeight),
                        Scale = 2.5 * currentScaling,
                        SourceLocation = currentSegment.SegmentSprite.SourceLocation,
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
            var sand = alternate ? GameColors.GRASS_ALT : GameColors.GRASS;
            var border = alternate ? GameColors.BORDER_RED : GameColors.WHITE;
            var road = alternate ? GameColors.LIGHT_GRAY : GameColors.GRAY;
            var lane = alternate ? GameColors.WHITE : GameColors.GRAY;

            if (finishStart)
            {
                road = GameColors.WHITE;
                lane = GameColors.WHITE;
                border = GameColors.WHITE;
            }

            // Draw Sand
            var sandBrush = new SolidBrush(ColorTranslator.FromHtml(sand));
            g.FillRectangle(sandBrush, new Rectangle(0, (int)position2, Game.Width, (int)(position1 - position2)));

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
                DrawImage(g, source, dest);
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
            var dest = new Rectangle(_player.SpriteDestination.X, _player.SpriteDestination.Y, _player.SpriteSource.Width, _player.SpriteSource.Height);
            DrawImage(g, _player.SpriteSource, dest);
        }

        private void DrawHud(Graphics g)
        {
            var percent = (int)(Math.Round(_absoluteIndex / (_roadParams.Length - Game.DepthOfField) * 100));
            DrawString(g, $"{percent}%", 287, 1);

            Game.RaceTimeSpan = DateTime.Now - _raceStartTime;
            var currentTimeString = Game.RaceTimeSpan.ToString(@"mm\:ss\:ff");
            DrawString(g, currentTimeString, 1, 1);

            var speed = Math.Round(_player.Speed / _player.MaxSpeed * 200);
            DrawString(g, $"{speed}mph", 1, 10);

            if (IsOffRoad())
            {
                DrawString(g, "OFF", 1, 20);
            }

            var zoneIndex = (int)(Math.Floor((_absoluteIndex / _roadParams.NumberOfSegmentsPerZone) + 1));
            DrawString(g, $"ZONE {zoneIndex}", 262, 10);
        }
    }
}
