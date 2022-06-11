import React from 'react';
import { Draggable } from 'react-beautiful-dnd';
import { Label } from 'semantic-ui-react';
import { getItemStyle, getLabelStyle } from '../../utils/dragAndDropHelper';

const Item = ({
    item,
    selectionCount,
    index,
    isSelected,
    isGhosting,
    t,
    search,
    toggleSelection,
    toggleSelectionInGroup,
    multiSelectTo,
    doubleClick,
    droppableId,
    isDragging,
}) => {
    const onClick = event => {
        if (event.defaultPrevented) {
            return;
        }

        if (event.button !== 0) {
            return;
        }

        // marking the event as used
        event.preventDefault();

        performAction(event);
    };

    const onTouchEnd = event => {
        if (event.defaultPrevented) {
            return;
        }

        // marking the event as used
        // we would also need to add some extra logic to prevent the click
        // if this element was an anchor
        event.preventDefault();
        toggleSelectionInGroup(item);
    };

    const onKeyDown = (event, provided, snapshot) => {
        if (event.defaultPrevented) {
            return;
        }

        if (snapshot.isDragging) {
            return;
        }

        if (event.keyCode !== 13) {
            return;
        }

        // we are using the event for selection
        event.preventDefault();

        performAction(event);
    };

    const onDoubleClick = () => {
        doubleClick(item, droppableId, index);
    };

    // Determines if the platform specific toggle selection in group key was used
    const wasToggleInSelectionGroupKeyUsed = event => {
        const isUsingWindows = navigator.platform.indexOf('Win') >= 0;
        return isUsingWindows ? event.ctrlKey : event.metaKey;
    };

    // Determines if the multiSelect key was used
    const wasMultiSelectKeyUsed = event => event.shiftKey;

    const performAction = event => {
        if (wasToggleInSelectionGroupKeyUsed(event)) {
            toggleSelectionInGroup(item);
            return;
        }

        if (wasMultiSelectKeyUsed(event)) {
            multiSelectTo(item);
            return;
        }

        toggleSelection(item);
    };

    return (
        <React.Fragment>
            {t(item.id)
                .toLowerCase()
                .includes(search.toLowerCase()) ? (
                <Draggable draggableId={item.id} index={index}>
                    {(provided, snapshot) => {
                        const shouldShowSelection = snapshot.isDragging && selectionCount > 1;

                        return (
                            <div
                                ref={provided.innerRef}
                                {...provided.draggableProps}
                                {...provided.dragHandleProps}
                                style={getItemStyle(
                                    snapshot.isDragging,
                                    provided.draggableProps.style,
                                    isSelected,
                                    isGhosting,
                                )}
                                onDoubleClick={onDoubleClick}
                                onClick={onClick}
                                onTouchEnd={onTouchEnd}
                                onKeyDown={event => onKeyDown(event, provided, snapshot)}
                            >
                                <Label
                                    style={getLabelStyle(
                                        isSelected,
                                        isGhosting,
                                        isDragging,
                                    )}
                                >
                                    {t(item.content.displayNameKey)}
                                </Label>
                                {shouldShowSelection ? (
                                    <div
                                        style={{
                                            position: 'absolute',
                                            height: '30px',
                                            width: '30px',
                                            background: '#2a3a4e',
                                            color: 'rgba(255, 255, 255, 0.8)',
                                            top: '-15px',
                                            right: '-15px',
                                            borderRadius: '50%',
                                            display: 'flex',
                                            alignItems: 'center',
                                            justifyContent: 'center',
                                        }}
                                    >
                                        {selectionCount}
                                    </div>
                                ) : null}
                            </div>
                        );
                    }}
                </Draggable>
            ) : null}
        </React.Fragment>
    );
};

export default Item;
