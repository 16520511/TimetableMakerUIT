import React, { Component } from 'react';

export class Home extends Component {
    static displayName = Home.name;

    constructor(props) {
        super(props);
        this.state = { classCodes: "", numberOfTable: 0, testing: "", classType: "CQ", loading: false };
    }

    
    testing = (e) => {
        e.preventDefault();
        var array = this.state.testing.split(/\r?\n/);
        var results = {}
        for (let i = 0; i < array.length; i++) {
            let authors = array[i].split(" ").slice(0, -1);
            let year = array[i].split(" ").slice(-1);
            if (year >= 9 && year <= 11) {
                for (let j = 0; j < authors.length; j++) {
                    for (let z = j + 1; z < authors.length; z++) {
                        if (results.hasOwnProperty(`${authors[j]}${authors[z]}`))
                            results[`${authors[j]}${authors[z]}`] += 1;
                        else
                            results[`${authors[j]}${authors[z]}`] = 1;
                    }
                }
            }
        }
        console.log(results);
    }

    formSubmit = (e) => {
        e.preventDefault();
        this.setState({ numberOfTable: 0 })
        //Loại bỏ các dòng trống và trùng lặp
        var unfilter = this.state.classCodes.split(/\r?\n/);

        var classCodes = unfilter.filter(function (value, index, arr) {

            return value != "";

        });

        classCodes = [...new Set(classCodes)];
        console.log(classCodes);
        var dataToPost = JSON.stringify({ subjectCodes: classCodes, classType: this.state.classType });
        this.setState({ loading: true });
        //Request đến API
        fetch('api/Main/CreateTimetable', {
            method: 'POST',
            body: dataToPost,
            headers: {
                'Content-Type': 'application/json'
            },
        }).then(response => response.json())
            .then(data => {
                console.log(data);
                this.setState({ numberOfTable: data.length, loading: false });
                for (let weekIndex = 0; weekIndex < data.length; weekIndex++) {
                    let days = data[weekIndex];
                    for (let dayIndex = 2; dayIndex < days.length; dayIndex++) {
                        let passIndex = [];
                        let day = days[dayIndex];
                        if (dayIndex == 8) {
                            for (let i = 0; i < day.length; i++) {
                                let mclass = day[i];
                                if (mclass == null) continue;
                                let tbody = document.getElementById("tablebody" + weekIndex);
                                var tr = document.createElement("tr");
                                var td = document.createElement("td")
                                td.setAttribute("colspan", 8);

                                //Dữ liệu lớp
                                var classCode = document.createElement("p");
                                classCode.append(document.createTextNode(`${day[i].classCode}`));
                                td.append(classCode);
                                td.append(document.createTextNode(`${day[i].className}`));
                                var classTeacher = document.createElement("p");
                                classTeacher.append(document.createTextNode(`${day[i].classTeacher}`));
                                td.append(classTeacher);
                                //
                                td.setAttribute("class", "class-cell")
                                tr.append(td);
                                tbody.append(tr);
                            }
                            break;
                        }

                        for (let classIndex = 1; classIndex < day.length; classIndex++) {
                            if (day[classIndex] == null) {
                                continue;
                            }
                            else if (!passIndex.includes(classIndex)) {
                                let counter = 1;
                                for (let indexAfter = classIndex + 1; indexAfter < day.length; indexAfter++) {
                                    if (day[indexAfter] == null) break;
                                    if (day[indexAfter].classCode != day[classIndex].classCode) break;
                                    else {
                                        counter++;
                                        passIndex.push(indexAfter);
                                    }
                                }
                                let col = document.getElementById("row" + classIndex + "-table" + weekIndex);
                                var el = document.createElement("td");
                                //Dữ liệu lớp
                                var classCode = document.createElement("p");
                                classCode.append(document.createTextNode(`${day[classIndex].classCode}`));
                                el.append(classCode);
                                el.append(document.createTextNode(`${day[classIndex].className}`));
                                var classTeacher = document.createElement("p");
                                classTeacher.append(document.createTextNode(`${day[classIndex].classTeacher}`));
                                el.append(classTeacher);
                                //
                                el.setAttribute("rowspan", counter);
                                el.setAttribute("class", "class-cell")
                                let childCount = col.childElementCount;
                                while (childCount < dayIndex - 1) {
                                    col.append(document.createElement("td"));
                                    childCount++;
                                }
                                col.append(el);
                            }
                        }
                    }
                
                }
            }).catch(e => console.log(e));
        
    }

    render() {
        let loader = (
            <div class="text-center">
                <div class="spinner-border text-info" role="status">
                    <span class="sr-only">Loading...</span>
                </div>
            </div>);
        let tables = [];
        for (let i = 0; i < this.state.numberOfTable; i++) {
            tables.push(<div>
                <h4>TKB số {i+1}</h4>
                <table id={"table" + i} className='table table-striped'>
                <thead>
                    <tr>
                        <th></th>
                        <th>Thứ 2</th>
                        <th>Thứ 3</th>
                        <th>Thứ 4</th>
                        <th>Thứ 5</th>
                        <th>Thứ 6</th>
                        <th>Thứ 7</th>
                    </tr>
                </thead>
                <tbody id={"tablebody" + i}>
                    <tr id={"row1-table" + i}><td className="day-label">Tiết 1</td></tr>
                    <tr id={"row2-table" + i}><td className="day-label">Tiết 2</td></tr>
                    <tr id={"row3-table" + i}><td className="day-label">Tiết 3</td></tr>
                    <tr id={"row4-table" + i}><td className="day-label">Tiết 4</td></tr>
                    <tr id={"row5-table" + i}><td className="day-label">Tiết 5</td></tr>
                    <tr id={"row6-table" + i}><td className="day-label">Tiết 6</td></tr>
                    <tr id={"row7-table" + i}><td className="day-label">Tiết 7</td></tr>
                    <tr id={"row8-table" + i}><td className="day-label">Tiết 8</td></tr>
                    <tr id={"row9-table" + i}><td className="day-label">Tiết 9</td></tr>
                    <tr id={"row10-table" + i}><td className="day-label">Tiết 10</td></tr>
                </tbody>
                </table>
            </div>);
        }
        return (
            <div id="wrapper">
                <h1>Tạo lịch đăng ký học phần UIT</h1>
                <form onSubmit={this.formSubmit}>
                    <div class="form-group">
                        <label for="classCodes">Nhập mã môn học cần đăng kí (Mỗi dòng 1 môn)</label>
                        <textarea onChange={e => this.setState({classCodes: e.target.value})} required class="form-control" id="classCodes" rows="6"></textarea>
                    </div>
                    <div class="input-group mb-3">
                        <div class="input-group-prepend">
                            <label class="input-group-text" for="classTypeInput">Lớp</label>
                        </div>
                        <select onChange={e => this.setState({classType: e.target.value})} class="custom-select" id="classTypeInput">
                            <option value="CQ" selected>Chính quy</option>
                            <option value="CLC">Chất lượng cao</option>
                            <option value="TT">Chương trình tiên tiến</option>
                            <option value="TN">Chương trình tài năng</option>
                        </select>
                    </div>
                    <button type="submit" class="btn btn-primary mb-2">Submit</button>
                </form>
                {this.state.loading ? loader : tables}
            </div>
        );
    }
}
