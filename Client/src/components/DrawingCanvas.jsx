import React from 'react';

const DrawingCanvas = ({ shapes }) => {
  
  return (
    <div className='canvas-wrapper'>
     <svg
  
  className="drawing-canvas"
  xmlns="http://www.w3.org/2000/svg"
>
        {shapes.map((shape, index) => {
          switch (shape.type) {
            case "circle":
              return (
                <circle
                  key={index}
                  cx={shape.x}
                  cy={shape.y}
                  r={shape.radius}
                  fill={shape.color }
                />
              );
            case "rectangle":
              return (
                <rect
                  key={index}
                  x={shape.x}
                  y={shape.y}
                  width={shape.width}
                  height={shape.height}
                  fill={shape.color }
                />
              );
            case "line":
              return (
                <line
                  key={index}
                  x1={shape.x1}
                  y1={shape.y1}
                  x2={shape.x2}
                  y2={shape.y2}
                  fill={shape.color }
                  strokeWidth={shape.strokeWidth || 2}
                />
              );
            case "polygon": {
              const pointsAttr = Array.isArray(shape.points)
                ? shape.points
                  .map(p => `${p.X ?? 0},${p.Y ?? 0}`)
                  .join(" ")
                : "";

              return (
                <polygon
                  key={index}
                  points={pointsAttr}
                  fill={shape.color }
                  stroke={shape.stroke || "black"}
                  strokeWidth={shape.strokeWidth ?? 1}
                />
              );
            }
            case "arc": {
              const r = shape.radius ?? 0;
              const cx = shape.x ?? 0;
              const cy = shape.y ?? 0;

              const startX = cx + r;
              const startY = cy;
              const endX = cx - r;
              const endY = cy;

              const d = `M ${startX} ${startY} A ${r} ${r} 0 0 0 ${endX} ${endY}`;

              return (
                <path
                  key={index}
                  d={d}
                  stroke={shape.color || "black"}
                  fill="none"
                  strokeWidth="2"
                />
              );
            }

            case "ellipse":
              return (
                <ellipse
                  key={index}
                  cx={shape.cx}
                  cy={shape.cy}
                  rx={shape.rx}
                  ry={shape.ry}
                  fill={shape.color}
                />
              );
            default:
              return null;
          }
        })}
      </svg>
    </div>
  );
};

export default DrawingCanvas;
