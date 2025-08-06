import React, { useState, useEffect } from 'react';
import HeaderControls from './HeaderControls';
import DrawingCanvas from './DrawingCanvas';
import ChatPanel from './ChatPanel';
import '../css/Main.css';
import axios from 'axios';

const Main = () => {
    const [shapes, setShapes] = useState([]);
    const [undoStack, setUndoStack] = useState([]);
    const [redoStack, setRedoStack] = useState([]);
    const [canvasId, setCanvasId] = useState(null);
    const [drawingHistory, setDrawingHistory] = useState([]);
    const [allCanvases, setAllCanvases] = useState([]);
    const [currentCanvas, setCurrentCanvas] = useState(null);

    //שליפת כל הקנבסים מהשרת
    useEffect(() => {
        axios.get('https://localhost:7270/api/Canvas')
            .then(res => {
                if (res.status == 200) {
                    setAllCanvases(res.data);
                }
            });
    },[])

    const handleLoadDrawing = (drawing) => {
        setUndoStack([...undoStack, shapes]);
        setRedoStack([]);
        setShapes(drawing.shapes || []);
    };

    const handleAddShapesFromAI = (newShapes) => {
        setUndoStack([...undoStack, shapes]);
        setRedoStack([]);
        setShapes([...shapes, ...newShapes]);
    };
    //שליפת קנבס מסוים לפי בחירה בסלקטור
    useEffect(() => {
        console.log(currentCanvas);
        
        if (currentCanvas) {
            axios.get(`https://localhost:7270/api/Canvas/${currentCanvas.id}`)
                .then(res => {
                    console.log(res, "resss");
                    if (res.status === 200) {
                        const data = res.data.drawings;
                        setShapes(data || []);
                    }
                });
        }
    }, [currentCanvas]);

    return (
        <div className="Main">
            <HeaderControls
                shapes={shapes}
                setShapes={setShapes}
                canvasId={canvasId}
                setCanvasId={setCanvasId}
                undoStack={undoStack}
                setUndoStack={setUndoStack}
                redoStack={redoStack}
                setRedoStack={setRedoStack}
                onLoadDrawing={handleLoadDrawing}
                drawingHistory={drawingHistory}
                setDrawingHistory={setDrawingHistory}
                allCanvases={allCanvases}
                setCurrentCanvas={setCurrentCanvas}
                setAllCanvases={setAllCanvases}
            />
            <div className="main-body">
                <ChatPanel onReceiveShapes={handleAddShapesFromAI} shapes={shapes} />
                <DrawingCanvas shapes={shapes} />
            </div>
        </div>
    );
};

export default Main;
