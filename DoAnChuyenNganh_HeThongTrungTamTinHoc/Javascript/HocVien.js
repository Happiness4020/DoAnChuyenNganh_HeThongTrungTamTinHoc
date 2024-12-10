document.addEventListener("DOMContentLoaded", function () {
    const hocVienData = [
        { anh: "User.jpg", maHocVien: "2001215669", tenHocVien: "Bùi Khánh Duy", email: "buikhanhduy13082003@gmail.com" },
    ];

    const thongTinHocVien = document.getElementById("thongTinHocVien");

    hocVienData.forEach(hocvien => {
        const row = document.createElement("tr");

        const subjectCell = document.createElement("td");
        subjectCell.textContent = hocvien.anh;
        row.appendChild(subjectCell);

        const timeCell = document.createElement("td");
        timeCell.textContent = hocvien.maHocVien;
        row.appendChild(timeCell);

        const roomCell = document.createElement("td");
        roomCell.textContent = hocvien.tenHocVien;
        row.appendChild(roomCell);

        const emailCell = document.createElement("td");
        emailCell.textContent = hocvien.email;
        row.appendChild(emailCell);

        thongTinHocVien.appendChild(row);
    });

    let currentWeekStart = new Date(2024, 9, 28);
    const oneWeek = 7 * 24 * 60 * 60 * 1000;

    function formatDate(date) {
        return date.toLocaleDateString("vi-VN");
    }

    function renderWeekLabel() {
        const weekEnd = new Date(currentWeekStart.getTime() + oneWeek - 1);
        document.getElementById("currentWeek").textContent = `Tuần này: ${formatDate(currentWeekStart)} - ${formatDate(weekEnd)}`;
    }

    function renderSchedule() {
        document.getElementById("lichHocVien").innerHTML = "";
    }

    document.getElementById("prevWeek").addEventListener("click", function () {
        currentWeekStart = new Date(currentWeekStart.getTime() - oneWeek);
        renderWeekLabel();
        renderSchedule();
    });

    document.getElementById("nextWeek").addEventListener("click", function () {
        currentWeekStart = new Date(currentWeekStart.getTime() + oneWeek);
        renderWeekLabel();
        renderSchedule();
    });

    renderWeekLabel();
    renderSchedule();

    document.querySelectorAll('.class-box').forEach(function (element) {
        element.addEventListener('mouseenter', function (event) {
            let tooltipText = this.getAttribute('data-tooltip');

            if (tooltipText) {
                let tooltip = document.createElement('div');
                tooltip.className = 'tooltip';
                tooltip.innerHTML = tooltipText;
                document.body.appendChild(tooltip);

                let rect = event.target.getBoundingClientRect();
                tooltip.style.left = `${rect.left + window.scrollX + 20}px`;
                tooltip.style.top = `${rect.top + window.scrollY + 20}px`;
                tooltip.style.display = 'block';
            }

            this.addEventListener('mousemove', function (event) {
                tooltip.style.left = `${event.pageX + 20}px`;
                tooltip.style.top = `${event.pageY + 20}px`;
            });

            this.addEventListener('mouseleave', function () {
                tooltip.remove();
            });
        });
    });
});
