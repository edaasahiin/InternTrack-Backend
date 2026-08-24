import { useEffect, useState } from "react";

function TaskForm({ onTaskAdded }) {
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [status, setStatus] = useState("ToDo");
    const [internId, setInternId] = useState("");
    const [interns, setInterns] = useState([]);

    useEffect(() => {
        fetch("http://localhost:5053/api/interns")
            .then(response => response.json())
            .then(data => setInterns(data));
    }, []);

    function handleSubmit(event) {
        event.preventDefault();

        const newTask = {
            title,
            description,
            status,
            internId: Number(internId)
        };

        fetch("http://localhost:5053/api/tasks", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(newTask)
        })
            .then(async response => {
                const text = await response.text();

                console.log("STATUS:", response.status);
                console.log("BACKEND CEVABI:", text);

                if (!response.ok) {
                    throw new Error(text || "Görev eklenemedi.");
                }

                return text;
            })
            .then(() => {
                setTitle("");
                setDescription("");
                setStatus("ToDo");
                setInternId("");

                onTaskAdded();
            })
            .catch(error => {
                console.error("GÖREV EKLEME HATASI:", error);
            });
    } // <-- EKSİK OLAN PARANTEZ BURASI

    return (
        <div>
            <h3>Görev Ekle</h3>

            <form onSubmit={handleSubmit}>
                <input
                    placeholder="Görev Başlığı"
                    value={title}
                    onChange={e => setTitle(e.target.value)}
                    required
                />

                <input
                    placeholder="Açıklama"
                    value={description}
                    onChange={e => setDescription(e.target.value)}
                />

                <select
                    value={status}
                    onChange={e => setStatus(e.target.value)}
                >
                    <option value="ToDo">ToDo</option>
                    <option value="InProgress">In Progress</option>
                    <option value="Done">Done</option>
                </select>

                <select
                    value={internId}
                    onChange={e => setInternId(e.target.value)}
                    required
                >
                    <option value="">Stajyer Seç</option>

                    {interns.map(intern => (
                        <option key={intern.id} value={intern.id}>
                            {intern.name}
                        </option>
                    ))}
                </select>

                <button type="submit">
                    Görev Ekle
                </button>
            </form>
        </div>
    );
}

export default TaskForm;