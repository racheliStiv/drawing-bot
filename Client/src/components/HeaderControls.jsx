
import React from "react";
import "../css/HeaderControls.css";
import sweetAlert from "sweetalert2";
import axios from "axios";
const HeaderControls = ({
  shapes,
  setShapes,
  canvasId,
  setCanvasId,
  undoStack,
  setUndoStack,
  redoStack,
  setRedoStack,
  onLoadDrawing,
  drawingHistory,
  setDrawingHistory,
  allCanvases,
  setCurrentCanvas,
  setAllCanvases
}) => {

  const checkName = async () => {
    var nameToUse = "";
    if (canvasId == null) {
      // אין CanvasId – נבקש שם חדש מהמשתמש
      sweetAlert.fire({
        title: "הכנס שם ציור:",
        input: "text",
        showCancelButton: true,
      }).then(async (result) => {
        if (result.isConfirmed) {
          nameToUse = result.value;
          await saveCanvas(nameToUse);

        }
      });
    } else {
      nameToUse = allCanvases.find(c => c.id === canvasId)?.name;
      axios.put(`https://localhost:7270/api/Canvas/${canvasId}`, {
        id: canvasId,
        drawings: JSON.stringify(shapes),
      });
      // יש canvasId – נשתמש בשם הקנבס הקיים
    }
  };

  async function saveCanvas(name) {
    try {
      const res = await axios.post(`https://localhost:7270/api/Canvas/`, {
        canvasName: name,
        drawings: JSON.stringify(shapes),
      });

      if (res.status !== 200 && res.status !== 201) {
        throw new Error("שמירה נכשלה");
      }

      const saved = res.data;
      setCanvasId(saved.id);
      // setAllCanvases((prev) => [...prev, saved]);

      console.log("נשמר בהצלחה");
    } catch (err) {
      console.error("שגיאה בשמירה:", err);
      sweetAlert.fire("שגיאה", "שמירה נכשלה", "error");
    }
  }

  const resetCanvas = () => {
    console.log(canvasId);

    if (shapes.length === 0) {
      alert("הקנבס ריק, אין מה לנקות");

      return;
    }
    if (confirm("האם אתה בטוח שברצונך לנקות את הקנבס?")) {
      if (canvasId != null) {
        axios.delete(`https://localhost:7270/api/Canvas/${canvasId}`)
          .then(res => {
            console.log(res);
            if (res.status == 204) {
              console.log("הקנבס נמחק בהצלחה");
            }
            else { alert("ניקוי הקנבס נכשל"); return; }
          })
          .catch(err => console.error("ניקוי הקנבס נכשל", err));
      }
      setUndoStack([]);
      setShapes([]);
      setAllCanvases(prev => prev.filter(c => c.id !== canvasId));
      setCanvasId(null);
      setRedoStack([]);
    }
  };

  const undo = () => {
    if (undoStack.length > 0) {
      const prev = undoStack[undoStack.length - 1];
      setRedoStack([...redoStack, shapes]);
      setShapes(prev);
      setUndoStack(undoStack.slice(0, -1));
    }
  };

  const redo = () => {
    if (redoStack.length > 0) {
      const next = redoStack[redoStack.length - 1];
      setUndoStack([...undoStack, shapes]);
      setShapes(next);
      setRedoStack(redoStack.slice(0, -1));
    }
  };

  const startNewDrawing = async () => {
    if (shapes.length === 0) {
      sweetAlert.fire({
        icon: "warning",
        title: "הקנבס ריק",
        text: "אין מה להתחיל ציור חדש",
      });
      return;
    }
    const result = await sweetAlert.fire({
      title: "ציור חדש",
      text: "האם אתה בטוח שברצונך להתחיל ציור חדש?",
    });
    if (result.isConfirmed) {
      if (canvasId == null) {
        sweetAlert.fire({
          text: 'עליך לשמור את הציור הנוכחי לפני שתתחיל ציור חדש',
          icon: "warning",
        });
        checkName();
      }
      setShapes([]);
      setCanvasId(null);
      setUndoStack([]);
      setRedoStack([]);
    }
  };

  const selectCanvas = (e) => {
    const selectedId = parseInt(e.target.value, 10);
    const selectedCanvas = allCanvases.find((c) => c.id === selectedId);
    console.log(selectedCanvas);

    if (selectedCanvas) {
      // if (shapes.length > 0) {
      //   checkName();
      // }
      setCurrentCanvas(selectedCanvas);
      setCanvasId(selectedId);
    } else {
      // אם בחרו את הערך הריק או לא נמצא, אפשר לנקות
      setCurrentCanvas(null);
    }
  };

  return (

    <div className="action-buttons" style={{justifyItems:'left', display: 'flex', gap: '10px', alignItems: 'center', flexWrap: 'wrap' }}>
      <select
        defaultValue=""
        onChange={selectCanvas}
        style={{
          padding: '8px 12px',
          borderRadius: '8px',
          border: '1px solid #ccc',
          fontSize: '16px',
          width: '150px',
          cursor: 'pointer',
          backgroundColor: '#f0f0f0'
        }}
      >
        <option value="">בחר ציור...</option>
        {allCanvases.map((canvas) => (
          <option key={canvas.id} value={canvas.id}>
            {canvas.name}
          </option>
        ))}
      </select>

      <button
        onClick={checkName}
        style={{
          padding: '8px 12px',
          borderRadius: '8px',
          border: 'none',
          fontSize: '16px',
          cursor: 'pointer',
          width: '120px',
          backgroundColor: '#4CAF50', // ירוק
          color: 'white'
        }}
      >
        💾 שמור
      </button>

      <button
        onClick={resetCanvas}
        style={{
          padding: '8px 12px',
          borderRadius: '8px',
          border: 'none',
          fontSize: '16px',
          cursor: 'pointer',
          width: '120px',
          backgroundColor: '#f44336', // אדום
          color: 'white'
        }}
      >
        🗑️ נקה
      </button>

      <button
        onClick={undo}
        style={{
          padding: '8px 12px',
          borderRadius: '8px',
          border: 'none',
          fontSize: '16px',
          cursor: 'pointer',
          width: '120px',
          backgroundColor: '#2196F3', // כחול
          color: 'white'
        }}
      >
        ↩️ אנדו
      </button>

      <button
        onClick={redo}
        style={{
          padding: '8px 12px',
          borderRadius: '8px',
          border: 'none',
          fontSize: '16px',
          cursor: 'pointer',
          width: '120px',
          backgroundColor: '#9C27B0', // סגול
          color: 'white'
        }}
      >
        ↪️ רידו
      </button>

      <button
        onClick={startNewDrawing}
        style={{
          padding: '8px 12px',
          borderRadius: '8px',
          border: 'none',
          fontSize: '16px',
          cursor: 'pointer',
          width: '120px',
          backgroundColor: '#FF9800', // כתום
          color: 'white'
        }}
      >
        🎨 ציור חדש
      </button>
    </div>

  );
};

export default HeaderControls;
