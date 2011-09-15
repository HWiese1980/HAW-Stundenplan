// Learn more about F# at http://fsharp.net

module GeometricCalculations

let Distance (x1, y1) (x2, y2) : double =
    let xdist = x1 - x2
    let ydist = y1 - y2
    sqrt(xdist * xdist + ydist * ydist)
    
    
    