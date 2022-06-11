import React from 'react';

const snapshotMap = {};

class TableCell extends React.Component {
    componentDidMount() {
        const cellId = this.props.cellId;
        if (!snapshotMap[cellId]) {
            return;
        }

        if (!this.props.isDragging) {
            // cleanup the map if it is not being used
            delete snapshotMap[cellId];
            return;
        }

        this.applySnapshot(snapshotMap[cellId]);
    }

    getSnapshotBeforeUpdate(prevProps) {
        // we will be locking the dimensions of the dragging item on mount
        if (this.props.isDragging) {
            return null;
        }

        const isDragStarting = this.props.isDragOccurring && !prevProps.isDragOccurring;

        if (!isDragStarting) {
            return null;
        }

        return this.getSnapshot();
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        const ref = this.ref;
        if (!ref) {
            return;
        }

        if (snapshot) {
            this.applySnapshot(snapshot);
            return;
        }

        if (this.props.isDragOccurring) {
            return;
        }

        // inline styles not applied
        if (ref.style.width == null) {
            return;
        }

        // no snapshot and drag is finished - clear the inline styles
        ref.style.removeProperty('height');
        ref.style.removeProperty('width');
    }

    componentWillUnmount() {
        const snapshot = this.getSnapshot();
        if (!snapshot) {
            return;
        }
        snapshotMap[this.props.cellId] = snapshot;
    }

    getSnapshot = () => {
        if (!this.ref) {
            return null;
        }

        const { width, height } = this.ref.getBoundingClientRect();

        const snapshot = {
            width,
            height,
        };

        return snapshot;
    };

    applySnapshot = snapshot => {
        const ref = this.ref;

        if (!ref) {
            return;
        }

        if (ref.style.width === snapshot.width) {
            return;
        }

        ref.style.width = `${snapshot.width}px`;
        ref.style.height = `${snapshot.height}px`;
    };

    setRef = ref => {
        this.ref = ref;
    };

    render() {
        return (
            <>
                {this.props.isHeader ? (
                    <td key={this.props.cellId} ref={this.setRef} style={{background: 'white', fontWeight: '500'}}>
                        {this.props.children}
                    </td>
                ) : (
                    <td
                        key={this.props.cellId}
                        ref={this.setRef}
                        className={`${
                            this.props.isDragging || this.props.isSelected ? ' dragging-cell' : ''
                        }`}
                    >
                        {this.props.children}
                    </td>
                )}
            </>
        );
    }
}

export default TableCell;
