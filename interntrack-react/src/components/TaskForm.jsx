import { useEffect, useState } from "react";
import { api } from "../services/api";
import AlertMessage from "./AlertMessage";
import { getErrorMessage } from "../utils/getErrorMessage";

function TaskForm({ onTaskAdded }) {
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [status, setStatus] = useState("ToDo");
    const [internId, setInternId] = useState("");
    const [interns, setInterns] = useState([]);

    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        async function loadInterns() {
            try {
                const data = await api.get("/interns");
                setInterns(data ?? []);
            } catch (error) {
                setIsError(true);
                setMessage(getErrorMessage(error));
            }
        }

        loadInterns();
    }, []);

    async function handleSubmit(event) {
        event.preventDefault();

        setMessage("");
        setIsError(false);
        setIsSubmitting(true);

        const newTask = {
            title,
            description,
            status,
            internId: Number(internId)
        };

        try {
            const data = await api.post("/tasks", newTask);

            setIsError(false);
            setMessage(
                data?.message || "Görev başarıyla eklendi."
            );

            setTitle("");
            setDescription("");
            setStatus("ToDo");
            setInternId("");

            onTaskAdded();
        } catch (error) {
            setIsError(true);
            setMessage(getErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
    }

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
                        <option
                            key={intern.id}
                            value={intern.id}
                        >
                            {intern.name}
                        </option>
                    ))}
                </select>

                <button
                    type="submit"
                    disabled={isSubmitting}
                >
                    {isSubmitting
                        ? "Ekleniyor..."
                        : "Görev Ekle"}
                </button>
            </form>

            <AlertMessage
                message={message}
                isError={isError}
            />
        </div>
    );
}

export default TaskForm;