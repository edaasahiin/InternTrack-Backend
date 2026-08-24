import { useEffect, useState } from "react";
import InternForm from "../components/InternForm";
import InternList from "../components/InternList";

function InternsPage() {
    const [interns, setInterns] = useState([]);

    function loadInterns() {
        fetch("http://localhost:5053/api/interns")
            .then(response => response.json())
            .then(data => setInterns(data))
            .catch(error => {
                console.error("Stajyerler yüklenemedi:", error);
            });
    }

    useEffect(() => {
        loadInterns();
    }, []);

    return (
        <div>
            <h2>Stajyerler</h2>

            <InternForm onInternAdded={loadInterns} />

            <InternList
                interns={interns}
                onInternDeleted={loadInterns}
            />
        </div>
    );
}

export default InternsPage;