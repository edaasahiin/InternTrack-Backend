import { useEffect, useState } from "react";

function DepartmentsPage() {
    const [departments, setDepartments] = useState([]);
    const [name, setName] = useState("");

    function loadDepartments() {
        fetch("http://localhost:5053/api/departments")
            .then(response => response.json())
            .then(data => setDepartments(data));
    }

    useEffect(() => {
        loadDepartments();
    }, []);

    function handleSubmit(event) {
        event.preventDefault();

        const newDepartment = {
            name: name
        };

        fetch("http://localhost:5053/api/departments", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(newDepartment)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("Departman eklenemedi.");
                }

                return response.json();
            })
            .then(() => {
                setName("");
                loadDepartments();
            })
            .catch(error => console.error(error));
    }

    function deleteDepartment(id) {
        fetch(`http://localhost:5053/api/departments/${id}`, {
            method: "DELETE"
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("Departman silinemedi.");
                }

                loadDepartments();
            })
            .catch(error => {
                console.error(error);
            });
    }

    return (
        <div>
            <h2>Departmanlar</h2>

            <form onSubmit={handleSubmit}>
                <input
                    type="text"
                    placeholder="Departman Adı"
                    value={name}
                    onChange={e => setName(e.target.value)}
                    required
                />

                <button type="submit">
                    Departman Ekle
                </button>
            </form>

            <h3>Departman Listesi</h3>

            {departments.map(department => (
                <div className="department-card" key={department.id}>
                    {department.name}

                    <button
                        onClick={() => deleteDepartment(department.id)}
                    >
                        Sil
                    </button>
                </div>
            ))}
        </div>
    );
}

export default DepartmentsPage;